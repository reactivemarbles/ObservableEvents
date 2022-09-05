// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    internal class StaticEventGenerator : EventGeneratorBase
    {
        /// <inheritdoc />
        public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, Func<string, ITypeSymbol?> getSymbolOf)
        {
            var eventWrapperMembers = new List<PropertyDeclarationSyntax>();

            var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

            foreach (var eventDetail in item.GetEvents(getSymbolOf, true))
            {
                var eventWrapper = GenerateEventWrapperObservable(eventDetail, item.GenerateFullGenericName(), item.Name);

                if (eventWrapper != null)
                {
                    eventWrapperMembers.Add(eventWrapper);
                }
            }

            if (eventWrapperMembers.Count > 0)
            {
                var members = new[]
                {
                    ClassDeclaration(
                        "RxEvents",
                        new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword },
                        eventWrapperMembers.Where(x => x != null).Select(x => (MemberDeclarationSyntax)x!).ToList(),
                        1)
                };

                return NamespaceDeclaration(namespaceName, members, true);
            }

            return null;
        }
    }
}
