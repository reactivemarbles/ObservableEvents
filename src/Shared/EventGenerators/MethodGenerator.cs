// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    internal static class MethodGenerator
    {
        public static MethodDeclarationSyntax GenerateMethod(this INamedTypeSymbol declarationType)
        {
            return declarationType.IsStatic ? GenerateStaticMethod(declarationType) : GenerateInstanceMethod(declarationType);
        }

        private static MethodDeclarationSyntax GenerateStaticMethod(INamedTypeSymbol declarationType)
        {
            var eventsClassName = "global::" + declarationType.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat) + ".Rx" + declarationType.Name + "Events";
            var modifiers = new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword };
            var parameters = new[] { Parameter(declarationType.GenerateFullGenericName(), "item", new[] { SyntaxKind.ThisKeyword }) };
            var body = ArrowExpressionClause(ObjectCreationExpression(eventsClassName, new[] { Argument("item") }));
            var attributes = RoslynHelpers.GenerateObsoleteAttributeList(declarationType);

            return MethodDeclaration(attributes, modifiers, eventsClassName, "Events", parameters, 0, body)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declarationType.GetArityDisplayName()));
        }

        private static MethodDeclarationSyntax GenerateInstanceMethod(INamedTypeSymbol declarationType)
        {
            var eventsClassName = "global::" + declarationType.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat) + ".Rx" + declarationType.Name + "Events";
            var modifiers = new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword };
            var parameters = new[] { Parameter(declarationType.GenerateFullGenericName(), "item", new[] { SyntaxKind.ThisKeyword }) };
            var body = ArrowExpressionClause(ObjectCreationExpression(eventsClassName, new[] { Argument("item") }));
            var attributes = RoslynHelpers.GenerateObsoleteAttributeList(declarationType);

            return MethodDeclaration(attributes, modifiers, eventsClassName, "Events", parameters, 0, body)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declarationType.GetArityDisplayName()));
        }
    }
}
