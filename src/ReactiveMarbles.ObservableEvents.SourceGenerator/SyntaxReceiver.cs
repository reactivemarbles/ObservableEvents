// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Events { get; } = new List<InvocationExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not InvocationExpressionSyntax invocationExpression)
            {
                return;
            }

            switch (invocationExpression.Expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    HandleSimpleName(memberAccess.Name, invocationExpression);
                    break;
                case MemberBindingExpressionSyntax bindingAccess:
                    HandleSimpleName(bindingAccess.Name, invocationExpression);
                    break;
            }
        }

        private void HandleSimpleName(SimpleNameSyntax simpleName, InvocationExpressionSyntax invocationExpression)
        {
            var methodName = simpleName.Identifier.Text;

            if (string.Equals(methodName, nameof(Events)))
            {
                Events.Add(invocationExpression);
            }
        }
    }
}
