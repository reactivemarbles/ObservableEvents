// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    /// <summary>
    /// Helper methods associated with the roslyn template generators.
    /// </summary>
    internal static class RoslynHelpers
    {
        internal const string ObservableUnitName = "global::System.Reactive.Unit";
        internal const string VoidType = "System.Void";

        public static SymbolDisplayFormat SymbolDisplayFormat { get; } = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance);

        /// <summary>
        /// Gets an argument which access System.Reactive.Unit.Default member.
        /// </summary>
        public static ArgumentListSyntax ReactiveUnitArgumentList { get; } = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ObservableUnitName + ".Default"))));

        public static Func<INamedTypeSymbol, bool> HasEvents { get; } = symbol => symbol.GetMembers().OfType<IEventSymbol>().Any(x => x.DeclaredAccessibility == Accessibility.Public);
    }
}