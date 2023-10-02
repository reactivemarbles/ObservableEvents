// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.Tests;

/// <summary>
/// Tests for generators.
/// </summary>
public abstract partial class GeneratorLibraryTestsBase
{
    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// Creates the generator for testing.
    /// </summary>
    /// <returns>The generator.</returns>
    protected abstract Microsoft.CodeAnalysis.ISourceGenerator CreateGenerator();

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

            var generator = CreateGenerator();

            var sourceGeneratorUtility = new SourceGeneratorUtility(text => TestContext?.WriteLine(text));
            sourceGeneratorUtility.RunGeneratorInstance(
                compilation,
                generator,
                out _,
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

            var generator = CreateGenerator();
            var sourceGeneratorUtility = new SourceGeneratorUtility(text => TestContext?.WriteLine(text));
            sourceGeneratorUtility.RunGeneratorInstance(
                compilation,
                generator,
                out _,
                out _,
                out _,
                source);
        }
    }
}
