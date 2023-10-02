// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.Tests;

/// <summary>Partial which contains the test cases.</summary>
public abstract partial class GeneratorLibraryTestsBase
{
    /// <summary>
    /// Tests all the derived instances.
    /// </summary>
    /// <param name="nugetPackageName">The NuGet package name.</param>
    /// <param name="targetFramework">The target framework.</param>
    /// <returns>A task to monitor the usage.</returns>
    [TestMethod]
    [DataRow("splat", "netstandard2.0")]
    [DataRow("splat", "net6.0")]
    [DataRow("splat", "net7.0")]
    [DataRow("Avalonia", "netstandard2.0")]
    [DataRow("Avalonia", "net6.0")]
    [DataRow("Avalonia", "net7.0")]
    [DataRow("Microsoft.Maui.Sdk", "netstandard2.0")]
    [DataRow("Microsoft.Maui.Sdk", "net6.0")]
    [DataRow("Microsoft.Maui.Sdk", "net7.0")]
    [DataRow("Xamarin.Forms", "netstandard2.0")]
    [DataRow("Xamarin.Forms", "net6.0")]
    [DataRow("Xamarin.Forms", "net7.0")]
    [DataRow("Xamarin.Essentials", "netstandard2.0")]
    [DataRow("Xamarin.Essentials", "net6.0")]
    [DataRow("Xamarin.Essentials", "net7.0")]
    [DataRow("FluentAssertions", "netstandard2.0")]
    [DataRow("FluentAssertions", "net6.0")]
    [DataRow("FluentAssertions", "net7.0")]
    [DataRow("Prism.Forms", "netstandard2.0")]
    [DataRow("Prism.Forms", "net6.0")]
    [DataRow("Prism.Forms", "net7.0")]
    [DataRow("SkiaSharp", "netstandard2.0")]
    [DataRow("SkiaSharp", "net6.0")]
    [DataRow("SkiaSharp", "net7.0")]
    [DataRow("Shiny", "netstandard2.0")]
    [DataRow("Shiny", "net6.0")]
    [DataRow("Shiny", "net7.0")]
    [DataRow("Newtonsoft.Json", "netstandard2.0")]
    [DataRow("Newtonsoft.Json", "net6.0")]
    [DataRow("Newtonsoft.Json", "net7.0")]
    public Task TestDerived(string nugetPackageName, string targetFramework) => RunNuGetTests(nugetPackageName, targetFramework, DeriveEventsTest);

    /// <summary>
    /// Tests all the concrete classes.
    /// </summary>
    /// <param name="nugetPackageName">The NuGet package name.</param>
    /// <param name="targetFramework">The target framework.</param>
    /// <returns>A task to monitor the usage.</returns>
    [TestMethod]
    [DataRow("splat", "netstandard2.0")]
    [DataRow("splat", "net6.0")]
    [DataRow("splat", "net7.0")]
    [DataRow("Avalonia", "netstandard2.0")]
    [DataRow("Avalonia", "net6.0")]
    [DataRow("Avalonia", "net7.0")]
    [DataRow("Microsoft.Maui.Sdk", "netstandard2.0")]
    [DataRow("Microsoft.Maui.Sdk", "net6.0")]
    [DataRow("Microsoft.Maui.Sdk", "net7.0")]
    [DataRow("Xamarin.Forms", "netstandard2.0")]
    [DataRow("Xamarin.Forms", "net6.0")]
    [DataRow("Xamarin.Forms", "net7.0")]
    [DataRow("Xamarin.Essentials", "netstandard2.0")]
    [DataRow("Xamarin.Essentials", "net6.0")]
    [DataRow("Xamarin.Essentials", "net7.0")]
    [DataRow("FluentAssertions", "netstandard2.0")]
    [DataRow("FluentAssertions", "net6.0")]
    [DataRow("FluentAssertions", "net7.0")]
    [DataRow("Prism.Forms", "netstandard2.0")]
    [DataRow("Prism.Forms", "net6.0")]
    [DataRow("Prism.Forms", "net7.0")]
    [DataRow("SkiaSharp", "netstandard2.0")]
    [DataRow("SkiaSharp", "net6.0")]
    [DataRow("SkiaSharp", "net7.0")]
    [DataRow("Shiny", "netstandard2.0")]
    [DataRow("Shiny", "net6.0")]
    [DataRow("Shiny", "net7.0")]
    [DataRow("Newtonsoft.Json", "netstandard2.0")]
    [DataRow("Newtonsoft.Json", "net6.0")]
    [DataRow("Newtonsoft.Json", "net7.0")]
    public Task TestConcrete(string nugetPackageName, string targetFramework) => RunNuGetTests(nugetPackageName, targetFramework, ConcreteClassCreate);

    /// <summary>
    /// Tests all the concrete platform.
    /// </summary>
    /// <param name="platform">The platform name.</param>
    /// <returns>A task to monitor the usage.</returns>
    [TestMethod]
    [DataRow("MonoAndroid50")]
    [DataRow("MonoAndroid51")]
    [DataRow("MonoAndroid60")]
    [DataRow("MonoAndroid70")]
    [DataRow("MonoAndroid71")]
    [DataRow("MonoAndroid80")]
    [DataRow("MonoAndroid81")]
    [DataRow("MonoAndroid90")]
    [DataRow("MonoAndroid10.0")]
    [DataRow("MonoAndroid11.0")]
    [DataRow("MonoTouch10")]
    [DataRow("Xamarin.iOS10")]
    [DataRow("Xamarin.Mac20")]
    [DataRow("Xamarin.TVOS10")]
    [DataRow("Xamarin.WATCHOS10")]
    [DataRow("net462")]
    [DataRow("net48")]
    [DataRow("net6.0")]
    [DataRow("net7.0")]
    public Task TestPlatformConcrete(string platform)
    {
        var input = new InputAssembliesGroup();

        var frameworks = platform.ToFrameworks();
        var framework = frameworks[0];
        input.IncludeGroup.AddFiles(FileSystemHelpers.GetFilesWithinSubdirectories(framework.GetNuGetFrameworkFolders(), AssemblyHelpers.AssemblyFileExtensionsSet));

        return RunTests(input, frameworks, ConcreteClassCreate);
    }

    /// <summary>
    /// Tests all the derived platform.
    /// </summary>
    /// <param name="platform">The platform name.</param>
    /// <returns>A task to monitor the usage.</returns>
    [TestMethod]
    [DataRow("MonoAndroid50")]
    [DataRow("MonoAndroid51")]
    [DataRow("MonoAndroid60")]
    [DataRow("MonoAndroid70")]
    [DataRow("MonoAndroid71")]
    [DataRow("MonoAndroid80")]
    [DataRow("MonoAndroid81")]
    [DataRow("MonoAndroid90")]
    [DataRow("MonoAndroid10.0")]
    [DataRow("MonoAndroid11.0")]
    [DataRow("MonoTouch10")]
    [DataRow("Xamarin.iOS10")]
    [DataRow("Xamarin.Mac20")]
    [DataRow("Xamarin.TVOS10")]
    [DataRow("Xamarin.WATCHOS10")]
    [DataRow("net462")]
    [DataRow("net48")]
    [DataRow("net6.0")]
    [DataRow("net7.0")]
    public Task TestPlatformDerived(string platform)
    {
        var input = new InputAssembliesGroup();

        var frameworks = platform.ToFrameworks();
        var framework = frameworks[0];
        input.IncludeGroup.AddFiles(FileSystemHelpers.GetFilesWithinSubdirectories(framework.GetNuGetFrameworkFolders(), AssemblyHelpers.AssemblyFileExtensionsSet));

        return RunTests(input, frameworks, DeriveEventsTest);
    }
}
