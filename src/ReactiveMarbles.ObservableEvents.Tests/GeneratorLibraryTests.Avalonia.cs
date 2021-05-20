// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis;

using Xunit;

namespace ReactiveMarbles.ObservableEvents.Tests
{
    /// <summary>
    /// Generator tests for avalonia.
    /// </summary>
    public partial class GeneratorLibraryTests
    {
        [Fact]
        public void TestAvalonia()
        {
            foreach (var classDeclaration in typeof(Avalonia.Controls.CheckBox).Assembly.GetTypes()
                .Where(x =>
                    x.IsPublic &&
                    x.Namespace?.StartsWith("Avalonia.Controls") == true &&
                    x.GenericTypeArguments.Length == 0))
            {
                var eventGenerationCalls = new StringBuilder();

                var events = classDeclaration.GetEvents();

                if (events.Length == 0)
                {
                    continue;
                }

                foreach (var eventDeclaration in events)
                {
                    eventGenerationCalls.Append("      this.Events().").Append(eventDeclaration.Name).AppendLine(".Subscribe();");
                }

                var source = $@"
using System;
using System.Collections.Generic;
using System.Text;

using ReactiveMarbles.ObservableEvents;

using Avalonia.Controls;

namespace ReactiveMarbles.ObservableEvents.Tests
{{
    public class AvaloniaTestCode : {classDeclaration.Name}
    {{
        public AvaloniaTestCode()
        {{
{eventGenerationCalls}
        }}
    }}
}}
";

                var sourceGeneratorUtility = new SourceGeneratorUtility(_testOutputHelper);
                sourceGeneratorUtility.RunGenerator(
                    new[]
                    {
                        typeof(Avalonia.Controls.CheckBox),
                    },
                    out var compilationDiagnostics,
                    out var generatorDiagnostics,
                    source);

                compilationDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
                generatorDiagnostics.Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
            }
        }
    }
}
