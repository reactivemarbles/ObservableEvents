// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
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
        /// <param name="item">The symbol to generate for.</param>
        /// <returns>The new compilation unit.</returns>
        NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item);
    }
}
