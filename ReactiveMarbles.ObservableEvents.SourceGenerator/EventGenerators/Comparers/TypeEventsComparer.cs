using System.Collections.Generic;

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
