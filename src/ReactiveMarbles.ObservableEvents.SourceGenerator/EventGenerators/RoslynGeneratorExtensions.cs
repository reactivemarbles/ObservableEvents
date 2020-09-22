// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    internal static class RoslynGeneratorExtensions
    {
        private static SymbolDisplayFormat _symbolDisplayFormat = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        /// <summary>
        /// Generates a argument list for a single parameter.
        /// </summary>
        /// <param name="parameter">The parameter to generate the argument list for.</param>
        /// <returns>The argument list.</returns>
        public static ArgumentListSyntax GenerateArgumentList(this IParameterSymbol parameter) => ArgumentList(SingletonSeparatedList(Argument(IdentifierName(parameter.Name.GetKeywordSafeName()))));

        /// <summary>
        /// Generates a argument list for a tuple parameter.
        /// </summary>
        /// <param name="parameters">The parameters to generate the argument list for.</param>
        /// <returns>The argument list.</returns>
        public static ArgumentListSyntax GenerateTupleArgumentList(this IEnumerable<IParameterSymbol> parameters) => ArgumentList(SingletonSeparatedList(Argument(TupleExpression(SeparatedList(parameters.Select(x => Argument(IdentifierName(x.Name.GetKeywordSafeName()))))))));

        public static TypeSyntax GenerateTupleType(this IEnumerable<(ITypeSymbol Type, string Name)> types)
        {
            return TupleType(SeparatedList(types.Select(x => TupleElement(IdentifierName(x.Type.GenerateFullGenericName()), Identifier(x.Name.GetKeywordSafeName())))));
        }

        public static TypeArgumentListSyntax GenerateObservableTypeArguments(this IMethodSymbol method)
        {
            TypeArgumentListSyntax argumentList;

            // If we have no parameters, use the Unit type, if only one use the type directly, otherwise use a value tuple.
            if (method.Parameters.Length == 0)
            {
                argumentList = TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(RoslynHelpers.ObservableUnitName)));
            }
            else if (method.Parameters.Length == 1)
            {
                argumentList = TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(method.Parameters[0].Type.GenerateFullGenericName())));
            }
            else
            {
                argumentList = TypeArgumentList(SingletonSeparatedList<TypeSyntax>(TupleType(SeparatedList(method.Parameters.Select(x => TupleElement(IdentifierName(x.Type.GenerateFullGenericName())).WithIdentifier(Identifier(x.Name)))))));
            }

            return argumentList;
        }

        public static List<TypeEvents> GetOrderedTypeEvents(this IReadOnlyList<INamedTypeSymbol> namedTypes)
        {
            var orderedTypeDeclarations = new List<TypeEvents>(namedTypes.Count);
            for (int i = 0; i < namedTypes.Count; ++i)
            {
                var namedType = namedTypes[i];

                var members = namedType.GetMembers();
                var events = new List<IEventSymbol>(members.Length);

                for (int memberIndex = 0; memberIndex < members.Length; memberIndex++)
                {
                    var member = members[memberIndex];

                    if (member is IEventSymbol eventSymbol && eventSymbol.DeclaredAccessibility == Accessibility.Public)
                    {
                        var eventIndex = events.BinarySearch(eventSymbol, EventNameComparer.Default);

                        if (eventIndex < 0)
                        {
                            events.Insert(~eventIndex, eventSymbol);
                        }
                    }
                }

                if (events.Count == 0)
                {
                    continue;
                }

                var element = new TypeEvents(namedType, events);
                var index = orderedTypeDeclarations.BinarySearch(element, TypeEventsComparer.Default);

                if (index < 0)
                {
                    orderedTypeDeclarations.Insert(~index, element);
                }
            }

            return orderedTypeDeclarations;
        }

        public static TypeSyntax GenerateObservableType(this TypeArgumentListSyntax argumentList)
        {
            return QualifiedName(IdentifierName("global::System"), GenericName(Identifier("IObservable")).WithTypeArgumentList(argumentList));
        }

        public static TypeSyntax GenerateObservableType(this TypeSyntax argumentList)
        {
            return QualifiedName(IdentifierName("global::System"), GenericName(Identifier("IObservable")).WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(argumentList))));
        }

        public static PropertyDeclarationSyntax WithObsoleteAttribute(this PropertyDeclarationSyntax syntax, ISymbol eventDetails)
        {
            var attribute = GenerateObsoleteAttributeList(eventDetails);

            if (attribute == null)
            {
                return syntax;
            }

            return syntax.WithAttributeLists(SingletonList(attribute));
        }

        public static ClassDeclarationSyntax WithObsoleteAttribute(this ClassDeclarationSyntax syntax, ISymbol eventDetails)
        {
            var attribute = GenerateObsoleteAttributeList(eventDetails);

            if (attribute == null)
            {
                return syntax;
            }

            return syntax.WithAttributeLists(SingletonList(attribute));
        }

        public static MethodDeclarationSyntax WithObsoleteAttribute(this MethodDeclarationSyntax syntax, ISymbol eventDetails)
        {
            var attribute = GenerateObsoleteAttributeList(eventDetails);

            if (attribute == null)
            {
                return syntax;
            }

            return syntax.WithAttributeLists(SingletonList(attribute));
        }

        public static ParameterListSyntax GenerateMethodParameters(this IMethodSymbol method)
        {
            if (method.Parameters.Length == 0)
            {
                return ParameterList();
            }

            return ParameterList(
                SeparatedList(
                    method.Parameters.Select(
                        x => Parameter(Identifier(x.Name.GetKeywordSafeName()))
                            .WithType(IdentifierName(x.Type.GenerateFullGenericName())))));
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseTypesAndThis(this INamedTypeSymbol type)
        {
            var current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseTypesAndThis(this INamedTypeSymbol type, Func<INamedTypeSymbol?, bool> condition)
        {
            var current = type;
            while (current != null)
            {
                if (condition.Invoke(current))
                {
                    yield return current;
                }

                current = current.BaseType;
            }
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="currentType">The type to generate the arguments for.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this ITypeSymbol currentType)
        {
            var (isBuiltIn, typeName) = GetBuiltInType(currentType.ToDisplayString(_symbolDisplayFormat));
            var sb = new StringBuilder(!isBuiltIn ? "global::" + typeName : typeName);

            return sb.ToString();
            ////if (currentType is INamedTypeSymbol namedSymbol && namedSymbol.TypeArguments.Length > 0)
            ////{
            ////    sb.Append('<')
            ////        .Append(string.Join(", ", namedSymbol.TypeArguments.Select(GenerateFullGenericName)))
            ////        .Append('>');
            ////}

            ////return sb.ToString();
        }

        public static IEnumerable<INamedTypeSymbol>? GetBasesWithCondition(this INamedTypeSymbol symbol, Func<INamedTypeSymbol, bool> condition)
        {
            INamedTypeSymbol? current = symbol.BaseType;

            while (current != null)
            {
                if (condition.Invoke(current))
                {
                    yield return current;
                }

                current = current.BaseType;
            }
        }

        private static (bool IsInternalType, string TypeName) GetBuiltInType(string typeName)
        {
            if (TypesMetadata.FullToBuiltInTypes.TryGetValue(typeName, out var builtInName))
            {
                return (true, builtInName);
            }

            return (false, typeName);
        }

        /// <summary>
        /// Gets information about the event's obsolete information if any.
        /// </summary>
        /// <param name="eventDetails">The event details.</param>
        /// <returns>The event's obsolete information if there is any.</returns>
        private static AttributeListSyntax? GenerateObsoleteAttributeList(ISymbol eventDetails)
        {
            var obsoleteAttribute = eventDetails.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Name.Equals("System.ObsoleteAttribute", StringComparison.InvariantCulture) ?? false);

            if (obsoleteAttribute == null)
            {
                return null;
            }

            var message = obsoleteAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
            var isError = bool.Parse(obsoleteAttribute.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString() ?? bool.FalseString) ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;
            var attribute = Attribute(
                IdentifierName("global::System.ObsoleteAttribute"),
                AttributeArgumentList(SeparatedList(new[] { AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(message))), AttributeArgument(LiteralExpression(isError)) })));

            return AttributeList(SingletonSeparatedList(attribute));
        }

        private static string GetKeywordSafeName(this string name)
        {
            return TypesMetadata.CSharpKeywords.Contains(name) ? '@' + name : name;
        }
    }
}
