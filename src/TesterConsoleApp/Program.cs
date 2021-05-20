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

namespace TesterConsoleApp
{
    /// <summary>
    /// Holds the main execution point of the application.
    /// </summary>
    public static class Program
    {
        private static readonly string StartDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        private static readonly string FileName = Path.Combine(StartDirectory, @"../ReactiveMarbles.ObservableEvents.Example/MyForm.cs");

        /// <summary>
        /// Executes a main instance of the application.
        /// </summary>
        public static void Main()
        {
            var fileText = File.ReadAllText(FileName);

            var (diagnostics, output) = GetGeneratedOutput(fileText);

            if (diagnostics.Length > 0)
            {
                Console.WriteLine("Diagnostics:");
                foreach (var diag in diagnostics)
                {
                    Console.WriteLine("   " + diag.ToString());
                }

                Console.WriteLine();
                Console.WriteLine("Output:");
            }

            Console.WriteLine(output);
        }

        private static (ImmutableArray<Diagnostic> Diagnostics, string Output) GetGeneratedOutput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic)
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var diagnostics = compilation.GetDiagnostics();

            if (diagnostics.Any())
            {
                return (diagnostics, string.Empty);
            }

            var generator = new EventGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);

            return (diagnostics, outputCompilation.SyntaxTrees.Last().ToString());
        }
    }
}
