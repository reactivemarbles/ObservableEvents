// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis;

namespace ReactiveMarbles.ObservableEvents.Tests;

internal static class HelperExtensions
{
    public static bool IsObsolete(this MemberInfo memberInfo)
    {
        var obsolete = memberInfo.GetCustomAttribute<ObsoleteAttribute>();

        return obsolete is not null;
    }
}
