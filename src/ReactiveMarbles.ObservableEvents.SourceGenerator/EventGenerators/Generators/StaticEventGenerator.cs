// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
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
        /// <inheritdoc />
        public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, bool generateEmpty)
        {
            var eventWrapperMembers = new List<PropertyDeclarationSyntax>();

            var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

            foreach (var eventDetail in item.GetEvents())
            {
                var eventWrapper = GenerateEventWrapperObservable(eventDetail, item.GenerateFullGenericName(), item.Name);

                if (eventWrapper != null)
                {
                    eventWrapperMembers.Add(eventWrapper);
                }
            }

            if (eventWrapperMembers.Count > 0)
            {
                var members = ClassDeclaration("RxEvents")
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A class that contains extension methods to wrap events contained within static classes within the {0} namespace.", namespaceName))
                    .WithMembers(List<MemberDeclarationSyntax>(eventWrapperMembers.Where(x => x != null).Select(x => x!)));

                return NamespaceDeclaration(IdentifierName(namespaceName))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(members));
            }

            return null;
        }
    }
}
