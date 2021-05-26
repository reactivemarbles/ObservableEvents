// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using ReactiveMarbles.ObservableEvents.SourceGenerator;

using Xunit.Abstractions;

namespace ReactiveMarbles.ObservableEvents.Tests
{
    internal class SourceGeneratorUtility
    {
        private static readonly Dictionary<string, MetadataReference> DomainAssemblyReferences =
            AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).Distinct(AssemblyEqualityComparer.Default).ToDictionary(x => x.FullName, x => (MetadataReference)MetadataReference.CreateFromFile(x.Location));

        private static readonly Dictionary<string, Assembly> DomainAssemblies =
            AppDomain.CurrentDomain.GetAssemblies().Distinct(AssemblyEqualityComparer.Default).ToDictionary(x => x.FullName);

        private static readonly MetadataReference[] SystemAssemblyReferences;
        private ITestOutputHelper _testOutputHelper;

        static SourceGeneratorUtility()
        {
            var assemblies = new HashSet<MetadataReference>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.FullName.StartsWith("System") && !assembly.FullName.StartsWith("mscordlib") && !assembly.FullName.StartsWith("netstandard"))
                {
                    continue;
                }

                if (assembly.IsDynamic)
                {
                    continue;
                }

                assemblies.Add(DomainAssemblyReferences[assembly.FullName]);
            }

            SystemAssemblyReferences = assemblies.ToArray();
        }

        public SourceGeneratorUtility(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void RunGenerator(Type[] types, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, params string[] sources)
        {
            var compilation = CreateCompilation(types, sources);

            var newCompilation = RunGenerators(compilation, out generatorDiagnostics, new EventGenerator());

            compilationDiagnostics = newCompilation.GetDiagnostics();

            ShouldHaveNoCompilerDiagnosticsWarningOrAbove(_testOutputHelper, newCompilation, compilationDiagnostics);
            ShouldHaveNoCompilerDiagnosticsWarningOrAbove(_testOutputHelper, compilation, generatorDiagnostics);
        }

        private static void ShouldHaveNoCompilerDiagnosticsWarningOrAbove(ITestOutputHelper output, Compilation compilation, IEnumerable<Diagnostic> diagnostics)
        {
            var compilationErrors = diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Select(x => $"{x.Location.SourceTree.FilePath} ({x.Location.GetLineSpan().StartLinePosition}): {x.GetMessage()}{Environment.NewLine}").ToList();

            var outputSources = string.Join(Environment.NewLine, compilation.SyntaxTrees.Select(x => $"// {x.FilePath}:{Environment.NewLine}{x}").Where(x => !x.Contains("The impementation should have been generated.")));

            if (compilationErrors.Count > 0)
            {
                output.WriteLine(outputSources);
                throw new InvalidOperationException(string.Join('\n', compilationErrors));
            }
        }

        /// <summary>
        /// Creates a compilation.
        /// </summary>
        /// <param name="types">The types to include.</param>
        /// <param name="sources">The source code to include.</param>
        /// <returns>The created compilation.</returns>
        private static Compilation CreateCompilation(Type[] types, params string[] sources)
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            if (assemblyPath == null || string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new InvalidOperationException("Could not find a valid assembly path.");
            }

            var assemblies = new HashSet<MetadataReference>();
            var processingStack = new Queue<Assembly>(types.Select(type => type.GetTypeInfo().Assembly));

            while (processingStack.Count != 0)
            {
                var assembly = processingStack.Dequeue();

                if (assembly.IsDynamic)
                {
                    continue;
                }

                var assemblyReference = DomainAssemblyReferences[assembly.FullName];

                if (assemblies.Contains(assemblyReference))
                {
                    continue;
                }

                assemblies.Add(assemblyReference);

                foreach (var referencedAssemblyLocation in assembly.GetReferencedAssemblies())
                {
                    if (!DomainAssemblies.TryGetValue(referencedAssemblyLocation.FullName, out var referencedAssembly))
                    {
                        continue;
                    }

                    if (referencedAssembly == null)
                    {
                        continue;
                    }

                    processingStack.Enqueue(referencedAssembly);
                }
            }

            return CSharpCompilation.Create(
                assemblyName: "compilation" + Guid.NewGuid(),
                syntaxTrees: sources.Select(x => CSharpSyntaxTree.ParseText(x, new CSharpParseOptions(LanguageVersion.Latest))),
                references: assemblies.Concat(SystemAssemblyReferences),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));
        }

        /// <summary>
        /// Executes the source generators.
        /// </summary>
        /// <param name="compilation">The target compilation.</param>
        /// <param name="diagnostics">The resulting diagnostics.</param>
        /// <param name="generators">The generators to include in the compilation.</param>
        /// <returns>The new compilation after the generators have executed.</returns>
        private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);
            return outputCompilation;
        }

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators) =>
            CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(generators),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: null);

        private sealed class AssemblyEqualityComparer : IEqualityComparer<Assembly>
        {
            private AssemblyEqualityComparer()
            {
            }

            public static AssemblyEqualityComparer Default { get; } = new AssemblyEqualityComparer();

            public bool Equals([AllowNull] Assembly x, [AllowNull] Assembly y)
            {
                if (x is null && y is null)
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return string.Equals(x.FullName, y.FullName, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode([DisallowNull] Assembly obj) => obj.FullName?.GetHashCode() ?? 0;
        }
    }
}
