// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    internal class TypeNameComparer : IEqualityComparer<INamedTypeSymbol>, IComparer<INamedTypeSymbol>
    {
        public static TypeNameComparer Default { get; } = new TypeNameComparer();

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

            return string.CompareOrdinal(x.Name, y.Name);
        }
    }
}
