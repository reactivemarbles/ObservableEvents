// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ReactiveMarbles.ObservableEvents.SourceGenerator;

namespace ReactiveMarbles.ObservableEvents.Tests;

/// <summary>
/// Tests the source generator based on Roslyn 3.8.
/// </summary>
[TestClass]
public class GeneratorLibraryTests38 : GeneratorLibraryTestsBase
{
    /// <inheritdoc/>
    protected override ISourceGenerator CreateGenerator() => new EventGenerator38();
}
