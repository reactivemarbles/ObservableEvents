// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    internal class InstanceEventGenerator : EventGeneratorBase
    {
        private const string DataFieldName = "_data";

        /// <inheritdoc />
        public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, bool generateEmpty)
        {
            var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

            var eventWrapper = GenerateEventWrapperClass(item, item.GetEvents(), generateEmpty);

            if (eventWrapper != null)
            {
               return NamespaceDeclaration(IdentifierName(namespaceName))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(eventWrapper));
            }

            return null;
        }

        private static ConstructorDeclarationSyntax GenerateEventWrapperClassConstructor(INamedTypeSymbol typeDefinition, bool hasBaseClass)
        {
            const string dataParameterName = "data";
            var constructor = ConstructorDeclaration(
                    Identifier("Rx" + typeDefinition.Name + "Events"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier(dataParameterName))
                                .WithType(
                                    IdentifierName(typeDefinition.GenerateFullGenericName())))))
                .WithBody(Block(SingletonList(
                    ExpressionStatement(
                        AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(DataFieldName), IdentifierName("data"))))))
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Initializes a new instance of the {0} class.", typeDefinition.ConvertToDocument(), (dataParameterName, "The class that is being wrapped.")));

            if (hasBaseClass)
            {
                constructor = constructor.WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer, ArgumentList(SingletonSeparatedList(Argument(IdentifierName(dataParameterName))))));
            }

            return constructor;
        }

        private static FieldDeclarationSyntax GenerateEventWrapperField(INamedTypeSymbol typeDefinition)
        {
            return FieldDeclaration(VariableDeclaration(IdentifierName(typeDefinition.GenerateFullGenericName()))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(DataFieldName)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));
        }

        private static ClassDeclarationSyntax GenerateEventWrapperClass(INamedTypeSymbol typeDefinition, IReadOnlyList<IEventSymbol> events, bool generateAlways)
        {
            var baseTypeDefinition = typeDefinition.GetBasesWithCondition(RoslynHelpers.HasEvents).FirstOrDefault();

            var members = new List<MemberDeclarationSyntax> { GenerateEventWrapperField(typeDefinition), GenerateEventWrapperClassConstructor(typeDefinition, baseTypeDefinition != null) };

            if (!generateAlways && events.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < events.Count; ++i)
            {
                var eventSymbol = events[i];
                var eventWrapper = GenerateEventWrapperObservable(eventSymbol, DataFieldName, null);

                if (eventWrapper == null)
                {
                    continue;
                }

                members.Add(eventWrapper);
            }

            var classDeclaration = ClassDeclaration("Rx" + typeDefinition.Name + "Events")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithMembers(List(members))
                .WithObsoleteAttribute(typeDefinition)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A class which wraps the events contained within the {0} class as observables.", typeDefinition.ConvertToDocument()));

            if (baseTypeDefinition != null)
            {
                classDeclaration = classDeclaration.WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName($"global::{baseTypeDefinition.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat)}.Rx{baseTypeDefinition.Name}Events")))));
            }

            return classDeclaration;
        }
    }
}
