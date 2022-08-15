// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using static ReactiveMarbles.RoslynHelpers.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    internal class StaticEventGenerator : EventGeneratorBase
    {
        /// <inheritdoc />
        public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item)
        {
            var eventWrapperMembers = new List<PropertyDeclarationSyntax>();

            var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

            foreach (var eventDetail in item.GetEvents(true))
            {
                var eventWrapper = GenerateEventWrapperObservable(eventDetail, item.GenerateFullGenericName(), null);

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
                        "Rx" + item.Name + "Events",
                        new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword },
                        eventWrapperMembers.Where(x => x != null).Select(x => (MemberDeclarationSyntax)x!).ToList(),
                        1),
                };

                return NamespaceDeclaration(namespaceName, members, true);
            }

            return null;
        }
    }
}
