// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.SourceGenerator;

/// <summary>
/// The event generator.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class EventGenerator4 : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(output => output.AddSource("TestExtensions.SourceGenerated.cs", Constants.ExtensionMethodText));

        var candidates = context.SyntaxProvider.CreateSyntaxProvider(
            (syntaxNode, _) => syntaxNode is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "Events" } or
                            MemberBindingExpressionSyntax { Name.Identifier.Text: "Events" }
            },
            (context, _) => (InvocationExpressionSyntax)context.Node);

        var inputs = candidates.Collect().Combine(context.CompilationProvider).Select((combined, _) => (Candidates: combined.Left, Compilation: combined.Right));

        context.RegisterSourceOutput(
            inputs,
            (sourceContext, collectedValues) => EventGeneratorHelpers.ExtractEvents((file, text) => sourceContext.AddSource(file, text), diag => sourceContext.ReportDiagnostic(diag), collectedValues.Compilation, collectedValues.Candidates));
    }
}
