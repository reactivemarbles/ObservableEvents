﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using ReactiveMarbles.ObservableEvents.Tests.Compilation;

using Xunit.Abstractions;

namespace ReactiveMarbles.ObservableEvents.Tests;

/// <summary>
/// The test cases for the generator tests.
/// </summary>
public partial class GeneratorLibraryTests
{
    /// <summary>
    /// Gets the testers for NuGet packages.
    /// </summary>
    public static IEnumerable<object[]> NuGetPackageTesters { get; } = new List<object[]>()
    {
        new object[] { "splat", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "splat", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "splat", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "splat", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "splat", "net6.0", "DeriveEventsTest3", },
        new object[] { "splat", "net6.0", "DeriveEventsTest4", },
        new object[] { "splat", "net6.0", "ConcreteClassCreate3", },
        new object[] { "splat", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Avalonia", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Avalonia", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Avalonia", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Avalonia", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Avalonia", "net6.0", "DeriveEventsTest3", },
        new object[] { "Avalonia", "net6.0", "DeriveEventsTest4", },
        new object[] { "Avalonia", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Avalonia", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Xamarin.Forms", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Xamarin.Forms", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Xamarin.Forms", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Xamarin.Forms", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Xamarin.Forms", "net6.0", "DeriveEventsTest3", },
        new object[] { "Xamarin.Forms", "net6.0", "DeriveEventsTest4", },
        new object[] { "Xamarin.Forms", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Xamarin.Forms", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Xamarin.Essentials", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Xamarin.Essentials", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Xamarin.Essentials", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Xamarin.Essentials", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Xamarin.Essentials", "net6.0", "DeriveEventsTest3", },
        new object[] { "Xamarin.Essentials", "net6.0", "DeriveEventsTest4", },
        new object[] { "Xamarin.Essentials", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Xamarin.Essentials", "net6.0", "ConcreteClassCreate4", },
        new object[] { "FluentAssertions", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "FluentAssertions", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "FluentAssertions", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "FluentAssertions", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "FluentAssertions", "net6.0", "DeriveEventsTest3", },
        new object[] { "FluentAssertions", "net6.0", "DeriveEventsTest4", },
        new object[] { "FluentAssertions", "net6.0", "ConcreteClassCreate3", },
        new object[] { "FluentAssertions", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Uno.UI", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Uno.UI", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Uno.UI", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Uno.UI", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Uno.UI", "net6.0", "DeriveEventsTest3", },
        new object[] { "Uno.UI", "net6.0", "DeriveEventsTest4", },
        new object[] { "Uno.UI", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Uno.UI", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Prism.Forms", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Prism.Forms", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Prism.Forms", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Prism.Forms", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Prism.Forms", "net6.0", "DeriveEventsTest3", },
        new object[] { "Prism.Forms", "net6.0", "DeriveEventsTest4", },
        new object[] { "Prism.Forms", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Prism.Forms", "net6.0", "ConcreteClassCreate4", },
        new object[] { "SkiaSharp", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "SkiaSharp", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "SkiaSharp", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "SkiaSharp", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "SkiaSharp", "net6.0", "DeriveEventsTest3", },
        new object[] { "SkiaSharp", "net6.0", "DeriveEventsTest4", },
        new object[] { "SkiaSharp", "net6.0", "ConcreteClassCreate3", },
        new object[] { "SkiaSharp", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Shiny", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Shiny", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Shiny", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Shiny", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Shiny", "net6.0", "DeriveEventsTest3", },
        new object[] { "Shiny", "net6.0", "DeriveEventsTest4", },
        new object[] { "Shiny", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Shiny", "net6.0", "ConcreteClassCreate4", },
        new object[] { "Newtonsoft.Json", "netstandard2.0", "DeriveEventsTest3", },
        new object[] { "Newtonsoft.Json", "netstandard2.0", "DeriveEventsTest4", },
        new object[] { "Newtonsoft.Json", "netstandard2.0", "ConcreteClassCreate3", },
        new object[] { "Newtonsoft.Json", "netstandard2.0", "ConcreteClassCreate4", },
        new object[] { "Newtonsoft.Json", "net6.0", "DeriveEventsTest3", },
        new object[] { "Newtonsoft.Json", "net6.0", "DeriveEventsTest4", },
        new object[] { "Newtonsoft.Json", "net6.0", "ConcreteClassCreate3", },
        new object[] { "Newtonsoft.Json", "net6.0", "ConcreteClassCreate4", },
    };

    /// <summary>
    /// Gets the testers for platforms.
    /// </summary>
    public static IEnumerable<object[]> PlatformTesters { get; } = new List<object[]>()
    {
        new object[] { "MonoAndroid50", "DeriveEventsTest3", },
        new object[] { "MonoAndroid50", "DeriveEventsTest4", },
        new object[] { "MonoAndroid50", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid50", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid51", "DeriveEventsTest3", },
        new object[] { "MonoAndroid51", "DeriveEventsTest4", },
        new object[] { "MonoAndroid51", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid51", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid60", "DeriveEventsTest3", },
        new object[] { "MonoAndroid60", "DeriveEventsTest4", },
        new object[] { "MonoAndroid60", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid60", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid70", "DeriveEventsTest3", },
        new object[] { "MonoAndroid70", "DeriveEventsTest4", },
        new object[] { "MonoAndroid70", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid70", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid71", "DeriveEventsTest3", },
        new object[] { "MonoAndroid71", "DeriveEventsTest4", },
        new object[] { "MonoAndroid71", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid71", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid80", "DeriveEventsTest3", },
        new object[] { "MonoAndroid80", "DeriveEventsTest4", },
        new object[] { "MonoAndroid80", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid80", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid81", "DeriveEventsTest3", },
        new object[] { "MonoAndroid81", "DeriveEventsTest4", },
        new object[] { "MonoAndroid81", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid81", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid90", "DeriveEventsTest3", },
        new object[] { "MonoAndroid90", "DeriveEventsTest4", },
        new object[] { "MonoAndroid90", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid90", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid10.0", "DeriveEventsTest3", },
        new object[] { "MonoAndroid10.0", "DeriveEventsTest4", },
        new object[] { "MonoAndroid10.0", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid10.0", "ConcreteClassCreate4", },
        new object[] { "MonoAndroid11.0", "DeriveEventsTest3", },
        new object[] { "MonoAndroid11.0", "DeriveEventsTest4", },
        new object[] { "MonoAndroid11.0", "ConcreteClassCreate3", },
        new object[] { "MonoAndroid11.0", "ConcreteClassCreate4", },
        new object[] { "MonoTouch10", "DeriveEventsTest3", },
        new object[] { "MonoTouch10", "DeriveEventsTest4", },
        new object[] { "MonoTouch10", "ConcreteClassCreate3", },
        new object[] { "MonoTouch10", "ConcreteClassCreate4", },
        new object[] { "Xamarin.iOS10", "DeriveEventsTest3", },
        new object[] { "Xamarin.iOS10", "DeriveEventsTest4", },
        new object[] { "Xamarin.iOS10", "ConcreteClassCreate3", },
        new object[] { "Xamarin.iOS10", "ConcreteClassCreate4", },
        new object[] { "Xamarin.Mac20", "DeriveEventsTest3", },
        new object[] { "Xamarin.Mac20", "DeriveEventsTest4", },
        new object[] { "Xamarin.Mac20", "ConcreteClassCreate3", },
        new object[] { "Xamarin.Mac20", "ConcreteClassCreate4", },
        new object[] { "Xamarin.TVOS10", "DeriveEventsTest3", },
        new object[] { "Xamarin.TVOS10", "DeriveEventsTest4", },
        new object[] { "Xamarin.TVOS10", "ConcreteClassCreate3", },
        new object[] { "Xamarin.TVOS10", "ConcreteClassCreate4", },
        new object[] { "Xamarin.WATCHOS10", "DeriveEventsTest3", },
        new object[] { "Xamarin.WATCHOS10", "DeriveEventsTest4", },
        new object[] { "Xamarin.WATCHOS10", "ConcreteClassCreate3", },
        new object[] { "Xamarin.WATCHOS10", "ConcreteClassCreate4", },
        new object[] { "net462", "DeriveEventsTest3", },
        new object[] { "net462", "DeriveEventsTest4", },
        new object[] { "net462", "ConcreteClassCreate3", },
        new object[] { "net462", "ConcreteClassCreate4", },
        new object[] { "net6.0", "DeriveEventsTest3", },
        new object[] { "net6.0", "DeriveEventsTest4", },
        new object[] { "net6.0", "ConcreteClassCreate3", },
        new object[] { "net6.0", "ConcreteClassCreate4", },
    };
}
