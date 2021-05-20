// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private ITestOutputHelper _testOutputHelper;

        public SourceGeneratorUtility(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void RunGenerator(Type[] types, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, params string[] sources)
        {
            var compilation = CreateCompilation(types, sources);

            var newCompilation = RunGenerators(compilation, out generatorDiagnostics, new EventGenerator());

            compilationDiagnostics = newCompilation.GetDiagnostics();

            var compilationErrors = compilationDiagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).Select(x => x.GetMessage()).ToList();
            var outputSources = string.Join(Environment.NewLine, newCompilation.SyntaxTrees.Select(x => x.ToString()).Where(x => !x.Contains("The impementation should have been generated.")));

            if (compilationErrors.Count > 0)
            {
                _testOutputHelper.WriteLine(outputSources);
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

            var assemblies = new HashSet<string>();
            var processingStack = new Queue<Assembly>(types.Select(type => type.GetTypeInfo().Assembly));
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            while (processingStack.Count != 0)
            {
                var assembly = processingStack.Dequeue();
                var assemblyLocaiton = assembly.Location;

                if (assemblies.Contains(assemblyLocaiton))
                {
                    continue;
                }

                assemblies.Add(assemblyLocaiton);

                foreach (var referencedAssemblyLocation in assembly.GetReferencedAssemblies())
                {
                    var referencedAssembly = Array.Find(domainAssemblies, x => x.FullName == referencedAssemblyLocation.FullName);

                    if (referencedAssembly == null)
                    {
                        continue;
                    }

                    processingStack.Enqueue(referencedAssembly);
                }
            }

            var defaultPaths = new[]
            {
                Path.Combine(assemblyPath, "mscorlib.dll"),
                Path.Combine(assemblyPath, "System.dll"),
                Path.Combine(assemblyPath, "System.Core.dll"),
                Path.Combine(assemblyPath, "System.ComponentModel.dll"),
                Path.Combine(assemblyPath, "System.Console.dll"),
                Path.Combine(assemblyPath, "System.Runtime.dll"),
                Path.Combine(assemblyPath, "netstandard.dll"),
                Path.Combine(assemblyPath, "System.Linq.Expressions.dll"),
                Path.Combine(assemblyPath, "System.ObjectModel.dll"),
                Path.Combine(assemblyPath, "System.Private.CoreLib.dll"),
            };

            foreach (var defaultPath in defaultPaths)
            {
                if (assemblies.Contains(defaultPath))
                {
                    continue;
                }

                assemblies.Add(defaultPath);
            }

            var references = assemblies.Select(x => MetadataReference.CreateFromFile(x));

            return CSharpCompilation.Create(
                assemblyName: "compilation" + Guid.NewGuid(),
                syntaxTrees: sources.Select(x => CSharpSyntaxTree.ParseText(x, new CSharpParseOptions(LanguageVersion.Latest))),
                references: references,
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
    }
}
