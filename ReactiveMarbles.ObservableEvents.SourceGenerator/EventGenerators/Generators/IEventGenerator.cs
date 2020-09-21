// Copyright (c) 2019 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    /// <summary>
    /// Generates based on events in the base code.
    /// </summary>
    internal interface IEventSymbolGenerator
    {
        /// <summary>
        /// Generates a compilation unit based on generating event observable wrappers.
        /// </summary>
        /// <param name="namespaceName">The name of the namespace.</param>
        /// <param name="namedTypes">The symbols to generate for.</param>
        /// <returns>The new compilation unit.</returns>
        NamespaceDeclarationSyntax? Generate(string namespaceName, IReadOnlyList<INamedTypeSymbol> namedTypes);
    }
}
