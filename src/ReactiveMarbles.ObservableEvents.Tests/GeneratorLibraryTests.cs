// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.Decompiler.TypeSystem;

using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;

using ReactiveMarbles.NuGet.Helpers;
using ReactiveMarbles.ObservableEvents.Tests.Compilation;

using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.ObservableEvents.Tests;

/// <summary>
/// Tests for generators.
/// </summary>
public partial class GeneratorLibraryTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GeneratorLibraryTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

    [Theory]
    [MemberData(nameof(NuGetPackageTesters))]
    public Task TestNuGet(string nugetPackageName, string targetFramework, string action)
    {
        return RunNuGetTests(nugetPackageName, targetFramework, _testOutputHelper, GetAction(action));
    }

    [Theory]
    [MemberData(nameof(PlatformTesters))]
    public Task TestPlatform(string platform, string action)
    {
        var input = new InputAssembliesGroup();

        var frameworks = platform.ToFrameworks();
        var framework = frameworks[0];
        input.IncludeGroup.AddFiles(FileSystemHelpers.GetFilesWithinSubdirectories(framework.GetNuGetFrameworkFolders(), AssemblyHelpers.AssemblyFileExtensionsSet));

        return RunTests(input, frameworks, _testOutputHelper, GetAction(action));
    }

    private static Action<ITestOutputHelper, EventBuilderCompiler> GetAction(string actionName) => actionName switch
    {
        nameof(DeriveEventsTest3) => DeriveEventsTest3,
        nameof(DeriveEventsTest4) => DeriveEventsTest4,
        nameof(ConcreteClassCreate3) => ConcreteClassCreate3,
        nameof(ConcreteClassCreate4) => ConcreteClassCreate4,
        _ => throw new InvalidOperationException("Unknown action name"),
    };

    private static async Task RunNuGetTests(string nugetPackageName, string targetFramework, ITestOutputHelper output, Action<ITestOutputHelper, EventBuilderCompiler> action)
    {
        var targetFrameworks = targetFramework.ToFrameworks();

#pragma warning disable CS0618 // Type or member is obsolete
        var library = new LibraryRange(nugetPackageName, VersionRange.AllStableFloating, LibraryDependencyTarget.Package);
#pragma warning restore CS0618 // Type or member is obsolete
        var inputGroup = await NuGetPackageHelper.DownloadPackageFilesAndFolder(library, targetFrameworks, packageOutputDirectory: null).ConfigureAwait(false);

        await RunTests(inputGroup, targetFrameworks, output, action).ConfigureAwait(false);
    }

    private static async Task RunTests(InputAssembliesGroup inputGroup, IReadOnlyList<NuGetFramework> targetFrameworks, ITestOutputHelper output, Action<ITestOutputHelper, EventBuilderCompiler> action)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var rxLibrary = new LibraryRange("System.Reactive", VersionRange.AllStableFloating, LibraryDependencyTarget.Package);
#pragma warning restore CS0618 // Type or member is obsolete

        var rxui = await NuGetPackageHelper.DownloadPackageFilesAndFolder(rxLibrary, targetFrameworks, packageOutputDirectory: null).ConfigureAwait(false);

        var framework = targetFrameworks[0];
        var compilation = new EventBuilderCompiler(inputGroup, rxui, framework);

        action(output, compilation);
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

    private static void DeriveEventsTest3(ITestOutputHelper testOutput, EventBuilderCompiler compilation) => DeriveEventsTest(
        testOutput,
        compilation,
        (gen, compiler, source) =>
            gen.RunGenerator3(
                    compiler,
                    out _,
                    out _,
                    source));

    private static void DeriveEventsTest4(ITestOutputHelper testOutput, EventBuilderCompiler compilation) => DeriveEventsTest(
        testOutput,
        compilation,
        (gen, compiler, source) =>
            gen.RunGenerator4(
                    compiler,
                    out _,
                    out _,
                    source));

    private static void DeriveEventsTest(ITestOutputHelper testOutput, EventBuilderCompiler compilation, Action<SourceGeneratorUtility, EventBuilderCompiler, string> runGenerator)
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

            var sourceGeneratorUtility = new SourceGeneratorUtility(testOutput);
            runGenerator(sourceGeneratorUtility, compilation, source);
        }
    }

    private static void ConcreteClassCreate3(ITestOutputHelper testOutput, EventBuilderCompiler compilation) => ConcreteClassCreate(
        testOutput,
        compilation,
        (gen, compiler, source) =>
            gen.RunGenerator3(
                    compiler,
                    out _,
                    out _,
                    source));

    private static void ConcreteClassCreate4(ITestOutputHelper testOutput, EventBuilderCompiler compilation) => ConcreteClassCreate(
        testOutput,
        compilation,
        (gen, compiler, source) =>
            gen.RunGenerator4(
                    compiler,
                    out _,
                    out _,
                    source));

    private static void ConcreteClassCreate(ITestOutputHelper testOutput, EventBuilderCompiler compilation, Action<SourceGeneratorUtility, EventBuilderCompiler, string> runGenerator)
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

            var sourceGeneratorUtility = new SourceGeneratorUtility(testOutput);
            runGenerator(
                sourceGeneratorUtility,
                compilation,
                source);
        }
    }
}
