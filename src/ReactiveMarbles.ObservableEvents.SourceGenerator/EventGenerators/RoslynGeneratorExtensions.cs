// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    internal static class RoslynGeneratorExtensions
    {
        /// <summary>
        /// Generates a argument list for a single parameter.
        /// </summary>
        /// <param name="parameter">The parameter to generate the argument list for.</param>
        /// <returns>The argument list.</returns>
        public static IReadOnlyCollection<ArgumentSyntax> GenerateArgumentList(this IParameterSymbol parameter) => new[] { Argument(parameter.Name.GetKeywordSafeName()) };

        /// <summary>
        /// Generates a argument list for a tuple parameter.
        /// </summary>
        /// <param name="parameters">The parameters to generate the argument list for.</param>
        /// <returns>The argument list.</returns>
        public static IReadOnlyCollection<ArgumentSyntax> GenerateTupleArgumentList(this IEnumerable<IParameterSymbol> parameters) => new[] { Argument(TupleExpression(parameters.Select(x => Argument(x.Name.GetKeywordSafeName())).ToList())) };

        public static TypeSyntax GenerateTupleType(this IReadOnlyCollection<(ITypeSymbol Type, string Name)> typeDescriptors)
        {
            var tupleTypes = new List<TupleElementSyntax>(typeDescriptors.Count);

            foreach (var typeDescriptor in typeDescriptors)
            {
                var typeName = typeDescriptor.Type.GenerateFullGenericName();
                var name = typeDescriptor.Name.GetKeywordSafeName();

                tupleTypes.Add(TupleElement(typeName, name));
            }

            return TupleType(tupleTypes);
        }

        public static TypeArgumentListSyntax GenerateObservableTypeArguments(this IMethodSymbol method)
        {
            TypeArgumentListSyntax argumentList;

            // If we have no parameters, use the Unit type, if only one use the type directly, otherwise use a value tuple.
            if (method.Parameters.Length == 0)
            {
                argumentList = TypeArgumentList(new[] { IdentifierName(RoslynHelpers.ObservableUnitName) });
            }
            else if (method.Parameters.Length == 1)
            {
                argumentList = TypeArgumentList(new[] { IdentifierName(method.Parameters[0].Type.GenerateFullGenericName()) });
            }
            else
            {
                argumentList = TypeArgumentList(new[] { TupleType(method.Parameters.Select(x => TupleElement(x.Type.GenerateFullGenericName(), x.Name)).ToList()) });
            }

            return argumentList;
        }

        public static IEnumerable<T> GetMembers<T>(this INamedTypeSymbol symbol)
            where T : ISymbol
        {
            var members = symbol.GetMembers();
            for (int i = 0; i < members.Length; ++i)
            {
                var member = members[i];

                if (member is not T eventSymbol)
                {
                    continue;
                }

                yield return eventSymbol;
            }
        }

        public static IEnumerable<IEventSymbol> GetEvents(this INamedTypeSymbol item, Func<string, ITypeSymbol?> getSymbolOf, bool staticEvents = false, bool includeInherited = true)
        {
            var baseClassWithEvents = includeInherited ? item.GetBasesWithCondition(RoslynHelpers.HasEvents) : Array.Empty<INamedTypeSymbol>();

            var itemsToProcess = new Stack<INamedTypeSymbol>(new[] { item }.Concat(baseClassWithEvents));

            var processedItems = new HashSet<string>();
            while (itemsToProcess.Count != 0)
            {
                var namedType = itemsToProcess.Pop();

                var members = namedType.GetMembers();

                for (int memberIndex = 0; memberIndex < members.Length; memberIndex++)
                {
                    var member = members[memberIndex];

                    if (member is not IEventSymbol eventSymbol)
                    {
                        continue;
                    }

                    if (processedItems.Contains(member.Name))
                    {
                        continue;
                    }

                    processedItems.Add(member.Name);

                    if (!staticEvents && eventSymbol.IsStatic)
                    {
                        continue;
                    }

                    if (staticEvents && !eventSymbol.IsStatic)
                    {
                        continue;
                    }

                    if (eventSymbol.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    var invokeMethod = ((INamedTypeSymbol)eventSymbol.OriginalDefinition.Type).DelegateInvokeMethod;

                    if (invokeMethod == null || ((invokeMethod.ReturnType as INamedTypeSymbol)?.IsGenericType ?? false))
                    {
                        continue;
                    }

                    if (invokeMethod.ReturnType.SpecialType == SpecialType.System_Void ||
                        SymbolEqualityComparer.Default.Equals(invokeMethod.ReturnType, getSymbolOf(typeof(Task).FullName)) ||
                        SymbolEqualityComparer.Default.Equals(invokeMethod.ReturnType, getSymbolOf(typeof(ValueTask).FullName)))
                    {
                        yield return eventSymbol;
                    }
                }
            }
        }

        public static TypeSyntax GenerateObservableType(this TypeArgumentListSyntax argumentList)
        {
            return QualifiedName(IdentifierName("global::System"), GenericName("IObservable").WithTypeArgumentList(argumentList));
        }

        public static TypeSyntax GenerateObservableType(this TypeSyntax argumentList)
        {
            return QualifiedName(IdentifierName("global::System"), GenericName("IObservable", new[] { argumentList }));
        }

        public static IReadOnlyList<ParameterSyntax> GenerateMethodParameters(this IMethodSymbol method)
        {
            if (method.Parameters.Length == 0)
            {
                return Array.Empty<ParameterSyntax>();
            }

            return method.Parameters.Select(
                        x => Parameter(x.Type.GenerateFullGenericName(), x.Name.GetKeywordSafeName())).ToList();
        }

        public static IEnumerable<T> GetBaseTypesAndThis<T>(this T type)
            where T : ITypeSymbol
        {
            T? current = type;
            while (current != null)
            {
                yield return current;
                current = (T?)current.BaseType;
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

        public static IReadOnlyList<TypeParameterSyntax> ToTypeParameters(this IEnumerable<INamedTypeSymbol> types)
        {
            return types.Select(x => TypeParameter(x.GenerateFullGenericName())).ToList();
        }

        /// <summary>
        /// Gets a string form of the type and generic arguments for a type.
        /// </summary>
        /// <param name="currentType">The type to generate the arguments for.</param>
        /// <returns>A type descriptor including the generic arguments.</returns>
        public static string GenerateFullGenericName(this ITypeSymbol currentType)
        {
            return currentType.ToDisplayString(RoslynHelpers.TypeFormat);
        }

        public static IEnumerable<INamedTypeSymbol> GetBasesWithCondition(this INamedTypeSymbol symbol, Func<INamedTypeSymbol, bool> condition)
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

        public static string GetArityDisplayName(this ITypeSymbol namedTypeSymbol)
        {
            var items = new Stack<ITypeSymbol>();

            ITypeSymbol current = namedTypeSymbol;

            while (current != null)
            {
                items.Push(current);
                current = current.ContainingType;
            }

            var stringBuilder = new StringBuilder();
            int i = 0;
            while (items.Count != 0)
            {
                var item = items.Pop();

                if (i != 0)
                {
                    stringBuilder.Append('.');
                }
                else
                {
                    stringBuilder.Append(namedTypeSymbol.ContainingNamespace.ToDisplayString()).Append('.');
                }

                stringBuilder.Append(item.Name);

                if (item is INamedTypeSymbol aritySymbol && aritySymbol.Arity > 0)
                {
                    stringBuilder.Append('`')
                        .Append(aritySymbol.Arity.ToString(CultureInfo.InvariantCulture));
                }

                i++;
            }

            return stringBuilder.ToString();
        }

        private static (bool IsInternalType, string TypeName) GetBuiltInType(string typeName)
        {
            if (TypesMetadata.FullToBuiltInTypes.TryGetValue(typeName, out var builtInName))
            {
                return (true, builtInName);
            }

            return (false, typeName);
        }

        private static string GetKeywordSafeName(this string name)
        {
            return TypesMetadata.CSharpKeywords.Contains(name) ? '@' + name : name;
        }
    }
}
