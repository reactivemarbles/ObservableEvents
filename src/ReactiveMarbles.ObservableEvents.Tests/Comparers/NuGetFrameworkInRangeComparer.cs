// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using NuGet.Frameworks;

namespace ReactiveMarbles.ObservableEvents.Tests.Comparers
{
    internal class NuGetFrameworkInRangeComparer : IComparer<NuGetFramework>, IEqualityComparer<NuGetFramework>
    {
        public static NuGetFrameworkInRangeComparer Default { get; } = new();

        /// <inheritdoc />
        public bool Equals(NuGetFramework? x, NuGetFramework? y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (!NuGetFramework.FrameworkNameComparer.Equals(x, y))
            {
                return false;
            }

            return x.Version >= y.Version;
        }

        /// <inheritdoc />
        public int GetHashCode(NuGetFramework obj)
        {
            return NuGetFramework.FrameworkNameComparer.GetHashCode(obj);
        }

        /// <inheritdoc />
        public int Compare(NuGetFramework? x, NuGetFramework? y)
        {
            switch (x)
            {
                case null when y is null:
                    return 0;
                case null:
                    return 1;
            }

            if (y is null)
            {
                return -1;
            }

            var result = StringComparer.OrdinalIgnoreCase.Compare(x.Framework, y.Framework);

            return result != 0 ? result : x.Version.CompareTo(y.Version);
        }
    }
}
