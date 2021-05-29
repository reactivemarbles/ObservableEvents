// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using NuGet.Packaging.Core;

namespace ReactiveMarbles.ObservableEvents.Tests.Comparers
{
    internal class PackageIdentityNameComparer : IEqualityComparer<PackageIdentity>
    {
        public static PackageIdentityNameComparer Default { get; } = new();

        /// <inheritdoc />
        public bool Equals(PackageIdentity? x, PackageIdentity? y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return ReferenceEquals(x, y) || StringComparer.OrdinalIgnoreCase.Equals(x.Id, y.Id);
        }

        /// <inheritdoc />
        public int GetHashCode(PackageIdentity obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Id);
        }
    }
}
