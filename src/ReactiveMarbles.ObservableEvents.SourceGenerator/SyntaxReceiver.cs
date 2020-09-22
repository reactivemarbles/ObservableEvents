// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators;
using ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator
{
    /// <summary>
    /// Created on demand before each generation pass.
    /// </summary>
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        /// <summary>
        /// Gets a list of candidate attributes specified globally within the assembly.
        /// </summary>
        public List<AttributeSyntax> CandidateAttributes { get; } = new List<AttributeSyntax>();

        /// <summary>
        /// Gets a list of candidate types that are specified directly.
        /// </summary>
        public List<TypeDeclarationSyntax> CandidateTypes { get; } = new List<TypeDeclarationSyntax>();

        /// <summary>
        /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation.
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // any field with at least one attribute is a candidate for property generation
            if (syntaxNode is AttributeSyntax attributeSyntax)
            {
                var attributeName = attributeSyntax.Name.ToFullString();
                if (attributeName == "EventsToObservablesAttribute" || attributeName == "EventsToObservables")
                {
                    CandidateAttributes.Add(attributeSyntax);
                }
            }

            if (syntaxNode is TypeDeclarationSyntax typeAttributeSyntax
                    && typeAttributeSyntax.AttributeLists.Count > 0)
            {
                CandidateTypes.Add(typeAttributeSyntax);
            }
        }
    }
}
