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
    internal class InstanceEventGenerator : EventGeneratorBase
    {
        private const string DataFieldName = "_data";

        /// <inheritdoc />
        public override NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, Func<string, ITypeSymbol?> getSymbolOf)
        {
            var namespaceName = item.ContainingNamespace.ToDisplayString(RoslynHelpers.SymbolDisplayFormat);

            var eventWrapperes = GenerateEventWrapperClasses(item, item.GetEvents(getSymbolOf).ToArray()).ToList();

            if (eventWrapperes.Count > 0)
            {
                return NamespaceDeclaration(namespaceName, eventWrapperes, true);
            }

            return null;
        }

        private static ConstructorDeclarationSyntax GenerateEventWrapperClassConstructor(INamedTypeSymbol typeDefinition)
        {
            const string dataParameterName = "data";
            string className = "Rx" + typeDefinition.Name + "Events";

            var constructorBlock = Block(
                new[]
                {
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, DataFieldName, "data"))
                },
                2);

            return ConstructorDeclaration(default, new[] { SyntaxKind.PublicKeyword }, new[] { Parameter(typeDefinition.GenerateFullGenericName(), dataParameterName) }, className, constructorBlock, 1)
                .WithLeadingTrivia(
                    XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Initializes a new instance of the {0} class.", className, (dataParameterName, "The class that is being wrapped.")));
        }

        private static FieldDeclarationSyntax GenerateEventWrapperField(INamedTypeSymbol typeDefinition)
        {
            return FieldDeclaration(
                typeDefinition.GenerateFullGenericName(),
                DataFieldName,
                new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword },
                1);
        }

        private static IEnumerable<ClassDeclarationSyntax> GenerateEventWrapperClasses(INamedTypeSymbol typeDefinition, IReadOnlyList<IEventSymbol> events)
        {
            var members = new List<MemberDeclarationSyntax> { GenerateEventWrapperField(typeDefinition), GenerateEventWrapperClassConstructor(typeDefinition) };

            if (events.Count == 0)
            {
                yield break;
            }

            var properties = new List<PropertyDeclarationSyntax>(events.Count);

            for (int i = 0; i < events.Count; ++i)
            {
                var eventSymbol = events[i];

                var eventWrapper = GenerateEventWrapperObservable(eventSymbol, DataFieldName, null);

                if (eventWrapper == null)
                {
                    continue;
                }

                properties.Add(eventWrapper);
            }

            var obsoleteList = RoslynHelpers.GenerateObsoleteAttributeList(typeDefinition);

            if (properties.Count > 0)
            {
                yield return ClassDeclaration(
                    "Rx" + typeDefinition.Name + "Events",
                    obsoleteList,
                    new[] { SyntaxKind.InternalKeyword },
                    members.Concat(properties).ToList(),
                    1);
            }
        }

        private class TypeArguments : IEquatable<TypeArguments>
        {
            public TypeArguments(INamedTypeSymbol[] typeArguments) => Types = typeArguments;

            public INamedTypeSymbol[] Types { get; }

            public bool Equals(TypeArguments other)
            {
                if (other is null)
                {
                    return false;
                }

                if (other.Types.Length != Types.Length)
                {
                    return false;
                }

                for (int i = 0; i < Types.Length; ++i)
                {
                    if (!TypeNameComparer.Default.Equals(Types[i], other.Types[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = 0;

                    foreach (var item in Types)
                    {
                        result = (result * 397) ^ TypeNameComparer.Default.GetHashCode(item);
                    }

                    return result;
                }
            }
        }
    }
}
