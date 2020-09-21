// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

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

        /// <summary>
        /// Gets an argument which access System.Reactive.Unit.Default member.
        /// </summary>
        public static ArgumentListSyntax ReactiveUnitArgumentList { get; } = SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(ObservableUnitName + ".Default"))));

        public static bool HasEvents(INamedTypeSymbol? symbol) => symbol?.GetMembers().OfType<IEventSymbol>().Where(x => x.DeclaredAccessibility == Accessibility.Public).Any() ?? false;
    }
}

