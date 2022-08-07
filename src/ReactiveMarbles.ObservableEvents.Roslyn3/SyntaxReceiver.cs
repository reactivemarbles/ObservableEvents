// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.SourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Events { get; } = new List<InvocationExpressionSyntax>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1513:Closing brace should be followed by blank line", Justification = "Pattern Matching unusual")]
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: nameof(Events) } or
                        MemberBindingExpressionSyntax { Name.Identifier.Text: nameof(Events) }
                } invocationExpression)
            {
                Events.Add(invocationExpression);
            }
        }
    }
}
