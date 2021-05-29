// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.Decompiler.TypeSystem;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;

using ReactiveMarbles.NuGet.Helpers;
using ReactiveMarbles.ObservableEvents.Tests.Compilation;

using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.ObservableEvents.Tests
{
    /// <summary>
    /// Tests for generators.
    /// </summary>
    public class GeneratorLibraryTests
    {
        private static readonly string[] PlatformNames = new[]
        {
            "MonoAndroid50",
            "MonoAndroid51",
            "MonoAndroid60",
            "MonoAndroid70",
            "MonoAndroid71",
            "MonoAndroid80",
            "MonoAndroid81",
            "MonoAndroid90",
            "MonoAndroid10.0",
            "MonoAndroid11.0",
            "MonoTouch10",
            "Xamarin.iOS10",
            "Xamarin.Mac20",
            "Xamarin.TVOS10",
            "Xamarin.WATCHOS10",
            "net461",
            "net5.0",
        };

        private static readonly string[] NuGetPackageNames = new[]
        {
            "splat",
            "Avalonia",
            "Xamarin.Forms",
            "Xamarin.Essentials",
            "FluentAssertions",
            "Uno.UI",
            "Prism.Forms",
            "SkiaSharp",
            "Shiny",
            "Newtonsoft.Json",
        };

        private static readonly string[] NuGetPackageFrameworks = new[]
        {
            "netstandard2.0",
            "net5.0"
        };

        private readonly ITestOutputHelper _testOutputHelper;

        public GeneratorLibraryTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        public static IEnumerable<object[]> Platforms { get; } = PlatformNames.Select(x => new object[] { x });

        public static IEnumerable<object[]> NuGetPackages { get; } = NuGetPackageNames.SelectMany(nuget => NuGetPackageFrameworks.Select(platform => new object[] { nuget, platform }));

        [Theory]
        [MemberData(nameof(NuGetPackages))]
        public Task TestDerived(string nugetPackageName, string targetFramework)
        {
            return RunNuGetTests(nugetPackageName, targetFramework, DeriveEventsTest);
        }

        [Theory]
        [MemberData(nameof(NuGetPackages))]
        public Task TestConcrete(string nugetPackageName, string targetFramework)
        {
            return RunNuGetTests(nugetPackageName, targetFramework, ConcreteClassCreate);
        }

        [Theory]
        [MemberData(nameof(Platforms))]
        public Task TestPlatformConcrete(string platform)
        {
            var input = new InputAssembliesGroup();

            var frameworks = platform.ToFrameworks();
            var framework = frameworks[0];
            input.IncludeGroup.AddFiles(FileSystemHelpers.GetFilesWithinSubdirectories(framework.GetNuGetFrameworkFolders(), AssemblyHelpers.AssemblyFileExtensionsSet));

            return RunTests(input, frameworks, ConcreteClassCreate);
        }

        [Theory]
        [MemberData(nameof(Platforms))]
        public Task TestPlatformDerived(string platform)
        {
            var input = new InputAssembliesGroup();

            var frameworks = platform.ToFrameworks();
            var framework = frameworks[0];
            input.IncludeGroup.AddFiles(FileSystemHelpers.GetFilesWithinSubdirectories(framework.GetNuGetFrameworkFolders(), AssemblyHelpers.AssemblyFileExtensionsSet));

            return RunTests(input, frameworks, DeriveEventsTest);
        }

        private static async Task RunNuGetTests(string nugetPackageName, string targetFramework, Action<EventBuilderCompiler> action)
        {
            var targetFrameworks = targetFramework.ToFrameworks();

#pragma warning disable CS0618 // Type or member is obsolete
            var library = new LibraryRange(nugetPackageName, VersionRange.AllStableFloating, LibraryDependencyTarget.Package);
#pragma warning restore CS0618 // Type or member is obsolete
            var inputGroup = await NuGetPackageHelper.DownloadPackageFilesAndFolder(library, targetFrameworks, packageOutputDirectory: null).ConfigureAwait(false);

            await RunTests(inputGroup, targetFrameworks, action).ConfigureAwait(false);
        }

        private static async Task RunTests(InputAssembliesGroup inputGroup, IReadOnlyList<NuGetFramework> targetFrameworks, Action<EventBuilderCompiler> action)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var rxLibrary = new LibraryRange("System.Reactive", VersionRange.AllStableFloating, LibraryDependencyTarget.Package);
#pragma warning restore CS0618 // Type or member is obsolete

            var rxui = await NuGetPackageHelper.DownloadPackageFilesAndFolder(rxLibrary, targetFrameworks, packageOutputDirectory: null).ConfigureAwait(false);

            var framework = targetFrameworks[0];
            var compilation = new EventBuilderCompiler(inputGroup, rxui, framework);

            action(compilation);
        }

        private static List<ITypeDefinition> GetValidTypes(ICompilation compilation, bool allowSealed)
        {
            var items = new List<ITypeDefinition>();
            foreach (var module in compilation.Modules)
            {
                foreach (var typeDefinition in module.TypeDefinitions)
                {
                    if (typeDefinition.IsSealed && !allowSealed)
                    {
                        continue;
                    }

                    if (typeDefinition.Accessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    if (typeDefinition.IsAbstract)
                    {
                        continue;
                    }

                    if (typeDefinition.Kind != TypeKind.Class)
                    {
                        continue;
                    }

                    if (!typeDefinition.GetConstructors().Any(x => x.Parameters.Count == 0 && x.Accessibility == Accessibility.Public && x.GetAttribute(KnownAttribute.Obsolete) is null))
                    {
                        continue;
                    }

                    if (typeDefinition.GetAttributes().Any(x => x.AttributeType.FullName.Equals("System.ObsoleteAttribute", StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    if (typeDefinition.GetAttribute(KnownAttribute.Obsolete, true) is not null)
                    {
                        continue;
                    }

                    if (typeDefinition.TypeParameterCount > 0)
                    {
                        continue;
                    }

                    var events = typeDefinition.GetEvents().Where(IsValidEvent);

                    if (events.Any())
                    {
                        items.Add(typeDefinition);
                    }
                }
            }

            return items;
        }

        private static List<IEvent> GetValidEvents(ITypeDefinition type) =>
            type.GetEvents(IsValidEvent).ToList();

        private static bool IsValidEvent(IEvent eventDetails)
        {
            if (eventDetails.IsStatic)
            {
                return false;
            }

            if (eventDetails.GetAttribute(KnownAttribute.Obsolete, true) is not null)
            {
                return false;
            }

            var delegateType = eventDetails.ReturnType;

            if (delegateType is null)
            {
                return false;
            }

            var invoke = delegateType.GetDelegateInvokeMethod();

            if (invoke is null)
            {
                return false;
            }

            if (invoke.ReturnType.FullName != "System.Void")
            {
                return false;
            }

            if (eventDetails.Accessibility != Accessibility.Public)
            {
                return false;
            }

            return true;
        }

        private void DeriveEventsTest(EventBuilderCompiler compilation)
        {
            foreach (var classDeclaration in GetValidTypes(compilation, false))
            {
                var eventGenerationCalls = new StringBuilder();

                var events = GetValidEvents(classDeclaration);

                if (events.Count == 0)
                {
                    continue;
                }

                foreach (var eventDeclaration in events)
                {
                    eventGenerationCalls.Append("          this.Events().").Append(eventDeclaration.Name).AppendLine(".Subscribe();");
                }

                if (eventGenerationCalls.Length == 0)
                {
                    continue;
                }

                var source = $@"
using System;
using System.Collections.Generic;
using System.Text;

using ReactiveMarbles.ObservableEvents;

namespace ReactiveMarbles.ObservableEvents.Tests
{{
    public class TestCode : {classDeclaration.FullName.Replace('+', '.')}
    {{
        public TestCode()
        {{
{eventGenerationCalls}
        }}
    }}
}}
";

                var sourceGeneratorUtility = new SourceGeneratorUtility(_testOutputHelper);
                sourceGeneratorUtility.RunGenerator(
                    compilation,
                    out _,
                    out _,
                    source);
            }
        }

        private void ConcreteClassCreate(EventBuilderCompiler compilation)
        {
            foreach (var classDeclaration in GetValidTypes(compilation, true))
            {
                var eventGenerationCalls = new StringBuilder();

                var events = GetValidEvents(classDeclaration);

                if (events.Count == 0)
                {
                    continue;
                }

                foreach (var eventDeclaration in events)
                {
                    eventGenerationCalls.Append("          Test.Events().").Append(eventDeclaration.Name).AppendLine(".Subscribe();");
                }

                if (eventGenerationCalls.Length == 0)
                {
                    continue;
                }

                var source = $@"
using System;
using System.Collections.Generic;
using System.Text;

using ReactiveMarbles.ObservableEvents;

namespace ReactiveMarbles.ObservableEvents.Tests
{{
    public class TestCode
    {{
        public {classDeclaration.FullName.Replace('+', '.')} Test {{ get; }} = new();
        public TestCode()
        {{
{eventGenerationCalls}
        }}
    }}
}}
";

                var sourceGeneratorUtility = new SourceGeneratorUtility(_testOutputHelper);
                sourceGeneratorUtility.RunGenerator(
                    compilation,
                    out _,
                    out _,
                    source);
            }
        }
    }
}
