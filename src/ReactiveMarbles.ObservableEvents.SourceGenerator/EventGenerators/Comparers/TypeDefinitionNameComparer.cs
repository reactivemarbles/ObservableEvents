﻿// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    /// <summary>
    /// A comparer which will compare <see cref="INamedTypeSymbol"/> names.
    /// </summary>
    internal class TypeDefinitionNameComparer : IEqualityComparer<INamedTypeSymbol>, IComparer<INamedTypeSymbol>
    {
        public static TypeDefinitionNameComparer Default { get; } = new TypeDefinitionNameComparer();

        /// <inheritdoc />
        public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y)
        {
            return StringComparer.Ordinal.Equals(x?.GenerateFullGenericName(), y?.GenerateFullGenericName());
        }

        /// <inheritdoc />
        public int GetHashCode(INamedTypeSymbol obj)
        {
            return StringComparer.Ordinal.GetHashCode(obj.GenerateFullGenericName());
        }

        /// <inheritdoc />
        public int Compare(INamedTypeSymbol? x, INamedTypeSymbol? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return string.CompareOrdinal(x.GenerateFullGenericName(), y.GenerateFullGenericName());
        }
    }
}
