// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            var eventsClassName = IdentifierName("global::" + declarationType.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat) + ".Rx" + declarationType.Name + "Events");
            return MethodDeclaration(eventsClassName, Identifier("Events"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("item"))
                        .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                        .WithType(IdentifierName(declarationType.GenerateFullGenericName())))))
                .WithExpressionBody(ArrowExpressionClause(
                    ObjectCreationExpression(eventsClassName)
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("item")))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .WithObsoleteAttribute(declarationType)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declarationType.GetArityDisplayName()));
        }

        private static MethodDeclarationSyntax GenerateInstanceMethod(INamedTypeSymbol declarationType)
        {
            var eventsClassName = IdentifierName("global::" + declarationType.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat) + ".Rx" + declarationType.Name + "Events");
            return MethodDeclaration(eventsClassName, Identifier("Events"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("item"))
                        .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                        .WithType(IdentifierName(declarationType.GenerateFullGenericName())))))
                .WithExpressionBody(ArrowExpressionClause(
                    ObjectCreationExpression(eventsClassName)
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("item")))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .WithObsoleteAttribute(declarationType)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declarationType.GetArityDisplayName()));
        }
    }
}
