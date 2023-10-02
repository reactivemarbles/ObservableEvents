// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Text;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator;

/// <summary>
/// Generates Observables from events in specified types and namespaces.
/// </summary>
[Generator]
public class EventGenerator38 : ISourceGenerator
{
    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context)
    {
        // add the attribute text.
        context.AddSource("TestExtensions.SourceGenerated.cs", SourceText.From(ClassConstants.ExtensionMethodText, Encoding.UTF8));

        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }

        var compilation = context.Compilation;
        var options = (compilation as CSharpCompilation)?.SyntaxTrees[0].Options as CSharpParseOptions;
        compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(ClassConstants.ExtensionMethodText, Encoding.UTF8), options));

        void AddSource(string sourceFileName, string source) => context.AddSource(sourceFileName, SourceText.From(source, Encoding.UTF8));
        Action<Diagnostic> addDiagnostic = context.ReportDiagnostic;

        var events = receiver.Events;

        EventGenerator.Generate(compilation, events, AddSource, addDiagnostic, CancellationToken.None);
    }

    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context) => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Events { get; } = new List<InvocationExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not InvocationExpressionSyntax invocationExpression)
            {
                return;
            }

            if (invocationExpression.IsCandidate())
            {
                Events.Add(invocationExpression);
            }
        }
    }
}
