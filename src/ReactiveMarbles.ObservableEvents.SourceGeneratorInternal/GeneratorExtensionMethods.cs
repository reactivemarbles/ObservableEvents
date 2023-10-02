// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveMarbles.ObservableEvents.SourceGenerator;

internal static class GeneratorExtensionMethods
{
    public static bool IsCandidate(this SyntaxNode syntaxNode)
    {
        if (syntaxNode is not InvocationExpressionSyntax invocationExpression)
        {
            return false;
        }

        string? methodName;
        switch (invocationExpression.Expression)
        {
            case MemberAccessExpressionSyntax memberAccess:
                methodName = memberAccess.Name.Identifier.Text;
                break;
            case MemberBindingExpressionSyntax bindingAccess:
                methodName = bindingAccess.Name.Identifier.Text;
                break;
            default:
                return false;
        }

        if (methodName is null)
        {
            return false;
        }

        return string.Equals(methodName, "Events", StringComparison.InvariantCulture);
    }
}
