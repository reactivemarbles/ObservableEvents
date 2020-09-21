// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    internal class StaticEventGenerator : EventGeneratorBase
    {
        /// <summary>
        /// Generate our namespace declarations. These will contain our helper classes.
        /// </summary>
        /// <param name="declarations">The declarations to add.</param>
        /// <returns>An array of namespace declarations.</returns>
        public override NamespaceDeclarationSyntax? Generate(string namespaceName, IReadOnlyList<INamedTypeSymbol> namedTypes)
        {
            var orderedTypeDeclarations = namedTypes.GetOrderedTypeEvents();

            if (orderedTypeDeclarations.Count == 0)
            {
                return null;
            }

            var eventWrapperMembers = new List<PropertyDeclarationSyntax>();
            foreach (var typeEvent in orderedTypeDeclarations)
            {
                foreach (var eventDetail in typeEvent.Events)
                {
                    var eventWrapper = GenerateEventWrapperObservable(eventDetail, typeEvent.Type.GenerateFullGenericName(), typeEvent.Type.Name);

                    if (eventWrapper != null)
                    {
                        eventWrapperMembers.Add(eventWrapper);
                    }
                }
            }

            if (eventWrapperMembers.Count > 0)
            {
                var members = ClassDeclaration("Events")
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A class that contains extension methods to wrap events contained within static classes within the {0} namespace.", namespaceName))
                    .WithMembers(List<MemberDeclarationSyntax>(eventWrapperMembers.Where(x => x != null).Select(x => x!)));

                return NamespaceDeclaration(IdentifierName(namespaceName))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(members));
            }

            return null;
        }
    }
}
