// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    internal class TypeEventsComparer : IEqualityComparer<TypeEvents>, IComparer<TypeEvents>
    {
        public static TypeEventsComparer Default { get; } = new TypeEventsComparer();

        public int Compare(TypeEvents x, TypeEvents y)
        {
            return TypeNameComparer.Default.Compare(x.Type, y.Type);
        }

        public bool Equals(TypeEvents x, TypeEvents y)
        {
            return TypeNameComparer.Default.Equals(x.Type, y.Type);
        }

        public int GetHashCode(TypeEvents obj)
        {
            return TypeNameComparer.Default.GetHashCode(obj.Type);
        }
    }
}
