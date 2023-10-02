// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators;

/// <summary>
/// Generates code syntax based on the Delegate based methodology
/// where we derive from a base class and override methods.
/// We provide an observable in this case.
/// </summary>
internal static class DelegateGenerator
{
    private static readonly QualifiedNameSyntax _subjectNamespace = QualifiedName("Pharmacist", "Common");
    private static readonly GenericNameSyntax _subjectType = GenericName("SingleAwaitSubject");

    /// <summary>
    /// Generate our namespace declarations. These will contain our helper classes.
    /// </summary>
    /// <param name="declarations">The declarations to add.</param>
    /// <returns>An array of namespace declarations.</returns>
    internal static IEnumerable<NamespaceDeclarationSyntax> Generate(IEnumerable<(INamedTypeSymbol TypeDefinition, bool IsAbstract, IReadOnlyCollection<IMethodSymbol> Methods)> declarations)
    {
        foreach (var groupedDeclarations in declarations.GroupBy(x => x.TypeDefinition.ContainingNamespace.Name).OrderBy(x => x.Key))
        {
            var namespaceName = groupedDeclarations.Key;
            var members = new List<ClassDeclarationSyntax>();

            members.AddRange(groupedDeclarations.OrderBy(x => x.TypeDefinition.Name).Select(x => GenerateClass(x.TypeDefinition, x.IsAbstract, x.Methods)));

            if (members.Count > 0)
            {
                yield return NamespaceDeclaration(namespaceName, members, true);
            }
        }
    }

    /// <summary>
    /// Generates our helper classes with the observables.
    /// </summary>
    /// <param name="typeDefinition">The type definition containing the information.</param>
    /// <param name="isAbstract">If the delegates are abstract.</param>
    /// <param name="methods">The methods to generate delegate overloads for.</param>
    /// <returns>The generated class declarations.</returns>
    private static ClassDeclarationSyntax GenerateClass(INamedTypeSymbol typeDefinition, bool isAbstract, IReadOnlyCollection<IMethodSymbol> methods)
    {
        var modifiers = typeDefinition.IsAbstract || isAbstract
            ? new[] { SyntaxKind.PublicKeyword, SyntaxKind.AbstractKeyword, SyntaxKind.PartialKeyword }
            : new[] { SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword };

        var baseClasses = new[] { SimpleBaseType(typeDefinition.GenerateFullGenericName()) };

        var attributes = RoslynHelpers.GenerateObsoleteAttributeList(typeDefinition);

        return ClassDeclaration(typeDefinition.Name + "Rx", attributes, modifiers, GenerateObservableMembers(methods).ToList(), baseClasses, 1)
            .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Wraps delegates events from {0} into Observables.", typeDefinition.GetArityDisplayName()));
    }

    private static IEnumerable<MemberDeclarationSyntax> GenerateObservableMembers(IReadOnlyCollection<IMethodSymbol> methods)
    {
        var methodDeclarations = new List<MethodDeclarationSyntax>(methods.Count);
        var fieldDeclarations = new List<FieldDeclarationSyntax>(methods.Count);
        var propertyDeclarations = new List<PropertyDeclarationSyntax>(methods.Count);

        foreach (var method in methods.OrderBy(y => y.Name))
        {
            var observableName = "_" + char.ToLowerInvariant(method.Name[0]) + method.Name.Substring(1);
            methodDeclarations.Add(GenerateMethodDeclaration(observableName, method));
            fieldDeclarations.Add(GenerateFieldDeclaration(observableName, method));
            propertyDeclarations.Add(GeneratePropertyDeclaration(observableName, method));
        }

        return fieldDeclarations.Cast<MemberDeclarationSyntax>().Concat(propertyDeclarations).Concat(methodDeclarations);
    }

    /// <summary>
    /// Produces the property declaration for the observable.
    /// </summary>
    /// <param name="observableName">The field name of the observable.</param>
    /// <param name="method">The method we are abstracting.</param>
    /// <returns>The property declaration.</returns>
    private static PropertyDeclarationSyntax GeneratePropertyDeclaration(string observableName, IMethodSymbol method)
    {
        var modifiers = new[] { SyntaxKind.PublicKeyword };
        var attributes = RoslynHelpers.GenerateObsoleteAttributeList(method);

        // Produces:
        // public System.IObservable<type> MethodNameObs => _observableName;
        return PropertyDeclaration(method.GenerateObservableTypeArguments().GenerateObservableType(), method.Name + "Obs", attributes, modifiers, ArrowExpressionClause(IdentifierName(observableName)), 2)
            .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Gets an observable which signals when the {0} method is invoked.", method.ConvertToDocument()));
    }

    /// <summary>
    /// Produces the field declaration which contains the subject.
    /// </summary>
    /// <param name="observableName">The field name of the observable.</param>
    /// <param name="method">The method we are abstracting.</param>
    /// <returns>The field declaration.</returns>
    private static FieldDeclarationSyntax GenerateFieldDeclaration(string observableName, IMethodSymbol method)
    {
        // Produces:
        // private readonly ReactiveUI.Events.SingleAwaitSubject<type> _methodName = new ReactiveUI.Events.SingleAwaitSubject<type>();
        var typeName = QualifiedName(_subjectNamespace, _subjectType.WithTypeArgumentList(method.GenerateObservableTypeArguments()));
        var modifiers = new[] { SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword };
        var attributes = RoslynHelpers.GenerateObsoleteAttributeList(method);

        var equalsClause = EqualsValueClause(ObjectCreationExpression(typeName));

        return FieldDeclaration(typeName, observableName, equalsClause, attributes, modifiers, 2);
    }

    private static MethodDeclarationSyntax GenerateMethodDeclaration(string observableName, IMethodSymbol method)
    {
        var attributes = RoslynHelpers.GenerateObsoleteAttributeList(method);
        var modifiers = new[] { SyntaxKind.PublicKeyword, SyntaxKind.OverrideKeyword };
        IReadOnlyCollection<ArgumentSyntax> arguments;

        // If we have any members call our observables with the parameters.
        if (method.Parameters.Length > 0)
        {
            // If we have only one member, just pass that directly, since our observable will have one generic type parameter.
            // If we have more than one parameter we have to pass them by value tuples, since observables only have one generic type parameter.
            if (method.Parameters.Length == 1)
            {
                arguments = method.Parameters[0].GenerateArgumentList();
            }
            else
            {
                arguments = method.Parameters.GenerateTupleArgumentList();
            }
        }
        else
        {
            arguments = RoslynHelpers.ReactiveUnitArgumentList;
        }

        var methodParameterList = method.GenerateMethodParameters();

        // Produces:
        // /// <inheritdoc />
        // public override void MethodName(params..) => _methodName.OnNext(...);
        var methodBody = ArrowExpressionClause(InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, observableName, "OnNext"), arguments));

        return MethodDeclaration(attributes, modifiers, "void", method.Name, methodParameterList, 2, methodBody)
            .WithLeadingTrivia(XmlSyntaxFactory.InheritdocSyntax);
    }
}
