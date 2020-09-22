// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    internal struct TypeEvents : IComparable<TypeEvents>
    {
        public TypeEvents(INamedTypeSymbol type, List<IEventSymbol> events)
        {
            Type = type;
            Events = events;
        }

        public INamedTypeSymbol Type { get; }

        public List<IEventSymbol> Events { get; }

        public int CompareTo(TypeEvents other)
        {
            return TypeEventsComparer.Default.Compare(other, this);
        }
    }
}
