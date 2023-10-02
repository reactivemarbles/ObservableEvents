// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using ICSharpCode.Decompiler.TypeSystem;

namespace ReactiveMarbles.SourceGenerator.TestNuGetHelper.Comparers
{
    internal class ModuleNameComparer : IEqualityComparer<IModule>
    {
        public static ModuleNameComparer Default { get; } = new();

        public bool Equals(IModule? x, IModule? y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return StringComparer.InvariantCulture.Equals(x.FullAssemblyName, y.FullAssemblyName);
        }

        public int GetHashCode(IModule obj) => obj.FullAssemblyName?.GetHashCode() ?? 0;
    }
}
