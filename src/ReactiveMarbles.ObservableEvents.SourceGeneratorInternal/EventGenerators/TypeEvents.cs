// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators;

internal struct TypeEvents : IComparable<TypeEvents>
{
    public TypeEvents(INamedTypeSymbol type, IEnumerable<IEventSymbol> events)
    {
        Type = type;
        Events = events.ToArray();
    }

    public INamedTypeSymbol Type { get; }

    public IReadOnlyList<IEventSymbol> Events { get; }

    public int CompareTo(TypeEvents other)
    {
        return TypeEventsComparer.Default.Compare(other, this);
    }
}
