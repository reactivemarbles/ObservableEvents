// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
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

        public override NamespaceDeclarationSyntax? Generate(string namespaceName, IReadOnlyList<INamedTypeSymbol> namedTypes)
        {
            var orderedTypeDeclarations = namedTypes.GetOrderedTypeEvents();

            if (orderedTypeDeclarations.Count == 0)
            {
                return null;
            }

            var members = new List<ClassDeclarationSyntax>(orderedTypeDeclarations.Count);

            members.Add(GenerateStaticClass(namespaceName, orderedTypeDeclarations));

            members.AddRange(orderedTypeDeclarations.Select(GenerateEventWrapperClass).Where(x => x != null));

            if (members.Count > 0)
            {
                return NamespaceDeclaration(IdentifierName(namespaceName))
                    .WithMembers(List<MemberDeclarationSyntax>(members));
            }

            return null;
        }

        private static ClassDeclarationSyntax GenerateStaticClass(string namespaceName, IEnumerable<TypeEvents> declarations)
        {
            // Produces:
            // public static class EventExtensions
            // contents of members above
            return ClassDeclaration("EventExtensions")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A class that contains extension methods to wrap events for classes contained within the {0} namespace.", namespaceName))
                .WithMembers(List<MemberDeclarationSyntax>(declarations.Select(declaration =>
                    {
                        var eventsClassName = IdentifierName("Rx" + declaration.Type.Name + "Events");
                        return MethodDeclaration(eventsClassName, Identifier("Events"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                            .WithParameterList(ParameterList(SingletonSeparatedList(
                                Parameter(Identifier("item"))
                                    .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                    .WithType(IdentifierName(declaration.Type.GenerateFullGenericName())))))
                            .WithExpressionBody(ArrowExpressionClause(
                                ObjectCreationExpression(eventsClassName)
                                    .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("item")))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            .WithObsoleteAttribute(declaration.Type)
                            .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("A wrapper class which wraps all the events contained within the {0} class.", declaration.Type.ConvertToDocument()));
                    })));
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

        private static ClassDeclarationSyntax GenerateEventWrapperClass(TypeEvents typeEvents)
        {
            var typeDefinition = typeEvents.Type;
            var baseTypeDefinition = typeDefinition.GetBasesWithCondition(RoslynHelpers.HasEvents).FirstOrDefault();
            var events = typeEvents.Events;

            var members = new List<MemberDeclarationSyntax> { GenerateEventWrapperField(typeDefinition), GenerateEventWrapperClassConstructor(typeDefinition, baseTypeDefinition != null) };

            foreach (var eventSymbol in typeEvents.Events)
            {
                var eventWrapper = GenerateEventWrapperObservable(eventSymbol, DataFieldName);

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
                classDeclaration = classDeclaration.WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName($"global::{baseTypeDefinition.ContainingNamespace.Name}.Rx{baseTypeDefinition.Name}Events")))));
            }

            return classDeclaration;
        }
    }
}
