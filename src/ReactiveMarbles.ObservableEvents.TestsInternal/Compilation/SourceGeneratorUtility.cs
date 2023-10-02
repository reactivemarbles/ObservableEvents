// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReactiveMarbles.SourceGenerator.TestNuGetHelper.Compilation
{
    /// <summary>
    /// The source generator utility which helps with getting NuGet packages and the source driver together.
    /// </summary>
    public class SourceGeneratorUtility
    {
        private readonly Action<string> _writeOutput;

        /// <summary>
        /// Initializes static members of the <see cref="SourceGeneratorUtility"/> class.
        /// </summary>
        static SourceGeneratorUtility()
        {
            var assemblies = new HashSet<MetadataReference>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName is null)
                {
                    continue;
                }

                if (!assembly.FullName.StartsWith("System") && !assembly.FullName.StartsWith("mscordlib") && !assembly.FullName.StartsWith("netstandard"))
                {
                    continue;
                }

                if (assembly.IsDynamic)
                {
                    continue;
                }

                assemblies.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceGeneratorUtility"/> class.
        /// </summary>
        /// <param name="writeOutput">Writes output for any errors found.</param>
        public SourceGeneratorUtility(Action<string> writeOutput) => _writeOutput = writeOutput ?? throw new ArgumentNullException(nameof(writeOutput));

        /// <summary>
        /// Runs the generator.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="generator">The source generator instance.</param>
        /// <param name="compilationDiagnostics">The diagnostics which are produced from the compiler.</param>
        /// <param name="generatorDiagnostics">The diagnostics which are produced from the generator.</param>
        /// <param name="generatorDriver">Output value for the driver.</param>
        /// <param name="sources">The source code files.</param>
        /// <returns>The returned source generator instance.</returns>
        public ISourceGenerator RunGeneratorInstance(EventBuilderCompiler compiler, ISourceGenerator generator, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, out GeneratorDriver generatorDriver, params string[] sources) =>
            RunGeneratorInstance(compiler, generator, out compilationDiagnostics, out generatorDiagnostics, out generatorDriver, out _, out _, sources);

        /// <summary>
        /// Runs the generator.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="generator">The source generator instance.</param>
        /// <param name="compilationDiagnostics">The diagnostics which are produced from the compiler.</param>
        /// <param name="generatorDiagnostics">The diagnostics which are produced from the generator.</param>
        /// <param name="generatorDriver">Output value for the driver.</param>
        /// <param name="sources">The source code files.</param>
        /// <returns>The returned source generator instance.</returns>
        public ISourceGenerator RunGeneratorInstance(EventBuilderCompiler compiler, ISourceGenerator generator, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, out GeneratorDriver generatorDriver, params (string FileName, string Source)[] sources) =>
            RunGeneratorInstance(compiler, generator, out compilationDiagnostics, out generatorDiagnostics, out generatorDriver, out _, out _, sources);

        /// <summary>
        /// Runs the generator.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="generator">The source generator instance.</param>
        /// <param name="compilationDiagnostics">The diagnostics which are produced from the compiler.</param>
        /// <param name="generatorDiagnostics">The diagnostics which are produced from the generator.</param>
        /// <param name="generatorDriver">Output value for the driver.</param>
        /// <param name="beforeCompilation">The compilation before the generator has run.</param>
        /// <param name="afterGeneratorCompilation">The compilation after the generator has run.</param>
        /// <param name="sources">The source code files.</param>
        /// <returns>The returned source generator instance.</returns>
        public ISourceGenerator RunGeneratorInstance(EventBuilderCompiler compiler, ISourceGenerator generator, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, out GeneratorDriver generatorDriver, out Microsoft.CodeAnalysis.Compilation beforeCompilation, out Microsoft.CodeAnalysis.Compilation afterGeneratorCompilation, params string[] sources) =>
            RunGeneratorInstance(compiler, generator, out compilationDiagnostics, out generatorDiagnostics, out generatorDriver, out beforeCompilation, out afterGeneratorCompilation, sources.Select(x => (FileName: "Unknown File", Source: x)).ToArray());

        /// <summary>
        /// Runs the generator.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="generator">The source generator instance.</param>
        /// <param name="compilationDiagnostics">The diagnostics which are produced from the compiler.</param>
        /// <param name="generatorDiagnostics">The diagnostics which are produced from the generator.</param>
        /// <param name="generatorDriver">Output value for the driver.</param>
        /// <param name="beforeCompilation">The compilation before the generator has run.</param>
        /// <param name="afterGeneratorCompilation">The compilation after the generator has run.</param>
        /// <param name="sources">The source code files.</param>
        /// <returns>The returned source generator instance.</returns>
        public ISourceGenerator RunGeneratorInstance(EventBuilderCompiler compiler, ISourceGenerator generator, out ImmutableArray<Diagnostic> compilationDiagnostics, out ImmutableArray<Diagnostic> generatorDiagnostics, out GeneratorDriver generatorDriver, out Microsoft.CodeAnalysis.Compilation beforeCompilation, out Microsoft.CodeAnalysis.Compilation afterGeneratorCompilation, params (string FileName, string Source)[] sources)
        {
            beforeCompilation = CreateCompilation(compiler, sources);

            afterGeneratorCompilation = RunGenerators(beforeCompilation, out generatorDiagnostics, out generatorDriver, generator);

            compilationDiagnostics = afterGeneratorCompilation.GetDiagnostics();

            ShouldHaveNoCompilerDiagnosticsWarningOrAbove(_writeOutput, afterGeneratorCompilation, compilationDiagnostics);
            ShouldHaveNoCompilerDiagnosticsWarningOrAbove(_writeOutput, beforeCompilation, generatorDiagnostics);

            return generator;
        }

        private static void ShouldHaveNoCompilerDiagnosticsWarningOrAbove(Action<string> output, Microsoft.CodeAnalysis.Compilation compilation, IEnumerable<Diagnostic> diagnostics)
        {
            var compilationErrors = diagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Select(x => $"// {x.Location.SourceTree?.FilePath} ({x.Location.GetLineSpan().StartLinePosition}): {x.GetMessage()}{Environment.NewLine}").ToList();

            var outputSources = string.Join(Environment.NewLine, compilation.SyntaxTrees.Select(x => $"// {x.FilePath}:{Environment.NewLine}{x}").Where(x => !x.Contains("The implementation should have been generated.")));

            if (compilationErrors.Count > 0)
            {
                output.Invoke(outputSources);
                throw new InvalidOperationException(string.Join(Environment.NewLine, compilationErrors));
            }
        }

        private static Microsoft.CodeAnalysis.Compilation CreateCompilation(EventBuilderCompiler compiler, params (string FileName, string Source)[] sources)
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            if (assemblyPath == null || string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new InvalidOperationException("Could not find a valid assembly path.");
            }

            var assemblies = new HashSet<MetadataReference>();

            assemblies.UnionWith(compiler.Modules.Where(x => x.PEFile is not null).Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)));
            assemblies.UnionWith(compiler.ReferencedModules.Where(x => x.PEFile is not null).Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)));
            assemblies.UnionWith(compiler.NeededModules.Where(x => x.PEFile is not null).Select(x => MetadataReference.CreateFromFile(x.PEFile!.FileName)));

            return CSharpCompilation.Create(
                "compilation" + Guid.NewGuid(),
                sources.Select(x => CSharpSyntaxTree.ParseText(x.Source, new CSharpParseOptions(LanguageVersion.Latest), x.FileName)),
                assemblies,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, deterministic: true));
        }

        private static Microsoft.CodeAnalysis.Compilation RunGenerators(Microsoft.CodeAnalysis.Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, out GeneratorDriver generatorDriver, params ISourceGenerator[] generators)
        {
            generatorDriver = CreateDriver(compilation, generators);
            generatorDriver = generatorDriver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);
            return outputCompilation;
        }

        private static GeneratorDriver CreateDriver(Microsoft.CodeAnalysis.Compilation compilation, params ISourceGenerator[] generators) =>
            CSharpGeneratorDriver.Create(
                generators,
                Array.Empty<AdditionalText>(),
                (CSharpParseOptions)compilation.SyntaxTrees.First().Options);
    }
}
