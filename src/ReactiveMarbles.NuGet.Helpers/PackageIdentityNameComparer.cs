// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using NuGet.Packaging.Core;

namespace ReactiveMarbles.NuGet.Helpers
{
    internal class PackageIdentityNameComparer : IEqualityComparer<PackageIdentity>
    {
        public static PackageIdentityNameComparer Default { get; } = new PackageIdentityNameComparer();

        /// <inheritdoc />
        public bool Equals(PackageIdentity x, PackageIdentity y)
        {
            if (x == y)
            {
                return true;
            }

            return StringComparer.OrdinalIgnoreCase.Equals(x?.Id, y?.Id);
        }

        /// <inheritdoc />
        public int GetHashCode(PackageIdentity obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Id);
        }
    }
}
