// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using NuGet.LibraryModel;
using NuGet.Packaging;

using ReactiveMarbles.NuGet.Helpers;

namespace ReactiveMarbles.ObservableEvents.Tests
{
    internal static class SourceGeneratorUtility
    {
        public static async Task<CompilationUnitSyntax> GetCompilationForNuGetLibrary(params LibraryRange[] nugetLibraries)
        {
            var packageFiles = await NuGetPackageHelper.DownloadPackageFilesAndFolder(nugetLibraries).ConfigureAwait(false);

            return SyntaxFactory.CompilationUnit();
        }

        public static async Task<(ImmutableArray<Diagnostic> Diagnostics, string Output)> GetGeneratedOutput<T>(string source, params LibraryRange[] nugetLibraries)
            where T : ISourceGenerator, new()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();

            var referenceFileLocations = new HashSet<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                {
                    referenceFileLocations.Add(assembly.Location);
                }
            }

            var packageFiles = await NuGetPackageHelper.DownloadPackageFilesAndFolder(nugetLibraries).ConfigureAwait(false);

            referenceFileLocations.AddRange(packageFiles.IncludeGroup.GetAllFileNames());
            referenceFileLocations.AddRange(packageFiles.SupportGroup.GetAllFileNames());

            foreach (var packageFile in referenceFileLocations)
            {
                references.Add(MetadataReference.CreateFromFile(packageFile));
            }

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var diagnostics = compilation.GetDiagnostics();

            if (diagnostics.Any())
            {
                return (diagnostics, string.Empty);
            }

            var generator = new T();

            var driver = CSharpGeneratorDriver.Create(new ISourceGenerator[] { generator });
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);

            return (diagnostics, outputCompilation.SyntaxTrees.Last().ToString());
        }
    }
}