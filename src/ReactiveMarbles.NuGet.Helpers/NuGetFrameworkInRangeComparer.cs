// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using NuGet.Frameworks;

namespace ReactiveMarbles.NuGet.Helpers
{
    internal class NuGetFrameworkInRangeComparer : IComparer<NuGetFramework>, IEqualityComparer<NuGetFramework>
    {
        public static NuGetFrameworkInRangeComparer Default { get; } = new NuGetFrameworkInRangeComparer();

        /// <inheritdoc />
        public bool Equals(NuGetFramework x, NuGetFramework y)
        {
            return !NuGetFramework.FrameworkNameComparer.Equals(x, y) ? false : x.Version >= y.Version;
        }

        /// <inheritdoc />
        public int GetHashCode(NuGetFramework obj)
        {
            return NuGetFramework.FrameworkNameComparer.GetHashCode(obj);
        }

        /// <inheritdoc />
        public int Compare(NuGetFramework x, NuGetFramework y)
        {
            var result = StringComparer.OrdinalIgnoreCase.Compare(x.Framework, y.Framework);

            return result != 0 ? result : x.Version.CompareTo(y.Version);
        }
    }
}
