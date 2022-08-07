// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.SourceGenerator;

/// <summary>
/// Generates Observables from events in specified types and namespaces.
/// </summary>
[Generator]
public class EventGenerator3 : ISourceGenerator
{
    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        // add the attribute text.
        context.AddSource("TestExtensions.SourceGenerated.cs", SourceText.From(Constants.ExtensionMethodText, Encoding.UTF8));

        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }

        var compilation = context.Compilation;
        var options = (compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions;
        compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(Constants.ExtensionMethodText, Encoding.UTF8), options));

        void AddText(string fileName, string text) => context.AddSource(fileName, SourceText.From(text, Encoding.UTF8));
        void ReportDiagnostic(Diagnostic diagnostic) => context.ReportDiagnostic(diagnostic);

        var events = receiver.Events;

        EventGeneratorHelpers.ExtractEvents(AddText, ReportDiagnostic, compilation, events);
    }

    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}
