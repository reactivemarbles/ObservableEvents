// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

using ReactiveMarbles.ObservableEvents.SourceGenerator;

namespace ReactiveMarbles.ObservableEvents.Tests;

/// <summary>
/// Tests the Roslyn 4.0 version of the generator.
/// </summary>
[TestClass]
public class GeneratorLibraryTests40 : GeneratorLibraryTestsBase
{
    /// <inheritdoc/>
    protected override ISourceGenerator CreateGenerator() => new EventGenerator40().AsSourceGenerator();
}
