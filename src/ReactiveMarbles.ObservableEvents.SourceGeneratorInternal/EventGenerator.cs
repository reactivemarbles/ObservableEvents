// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators;
using ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators;
using ReactiveMarbles.PropertyChanged.SourceGenerator;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator;

internal static class EventGenerator
{
    private static readonly InstanceEventGenerator _eventGenerator = new();
    private static readonly StaticEventGenerator _staticEventGenerator = new();

    public static void Generate(Compilation compilation, IReadOnlyList<InvocationExpressionSyntax> events, Action<string, string> addSource, Action<Diagnostic> addDiagnostic, CancellationToken token)
    {
        var extensionMethodInvocations = new List<MethodDeclarationSyntax>();
        var staticMethodInvocations = new List<MethodDeclarationSyntax>();

        GetAvailableTypes(compilation, events, out var instanceNamespaceList, out var staticNamespaceList, token);

        GenerateEvents(compilation, _staticEventGenerator, true, staticNamespaceList, addSource, addDiagnostic, staticMethodInvocations, token);
        GenerateEvents(compilation, _eventGenerator, false, instanceNamespaceList, addSource, addDiagnostic, extensionMethodInvocations, token);

        GenerateEventExtensionMethods(addSource, extensionMethodInvocations);
    }

    private static void GenerateEventExtensionMethods(Action<string, string> addSource, List<MethodDeclarationSyntax> methodInvocationExtensions)
    {
        var classDeclaration = ClassDeclaration("ObservableGeneratorExtensions", new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword }, methodInvocationExtensions, 1);

        var namespaceDeclaration = NamespaceDeclaration("ReactiveMarbles.ObservableEvents", new[] { classDeclaration }, true);

        var compilationUnit = GenerateCompilationUnit(namespaceDeclaration);

        if (compilationUnit == null)
        {
            return;
        }

        string sourceText = compilationUnit.ToFullString();
        addSource.Invoke("TestExtensions.FoundEvents.SourceGenerated.cs", sourceText);
    }

    private static void GetAvailableTypes(
        Compilation compilation,
        IReadOnlyList<InvocationExpressionSyntax> events,
        out List<(Location Location, INamedTypeSymbol NamedType)> instanceNamespaceList,
        out List<(Location Location, INamedTypeSymbol NamedType)> staticNamespaceList,
        CancellationToken token)
    {
        var observableGeneratorExtensions = compilation.GetTypeByMetadataName("ReactiveMarbles.ObservableEvents.ObservableGeneratorExtensions");

        if (observableGeneratorExtensions == null)
        {
            throw new InvalidOperationException("Cannot find ReactiveMarbles.ObservableEvents.ObservableGeneratorExtensions");
        }

        instanceNamespaceList = new List<(Location Location, INamedTypeSymbol NamedType)>();
        staticNamespaceList = new List<(Location Location, INamedTypeSymbol NamedType)>();

        foreach (var invocation in events)
        {
            token.ThrowIfCancellationRequested();
            var semanticModel = compilation.GetSemanticModel(invocation.SyntaxTree);

            if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, observableGeneratorExtensions))
            {
                continue;
            }

            if (methodSymbol.TypeArguments.Length != 1)
            {
                continue;
            }

            if (methodSymbol.TypeArguments[0] is not INamedTypeSymbol callingSymbol)
            {
                continue;
            }

            if (callingSymbol.IsGenericType)
            {
                callingSymbol = callingSymbol.OriginalDefinition;
            }

            var location = Location.Create(invocation.SyntaxTree, invocation.Span);

            instanceNamespaceList.Add((location, callingSymbol));
        }

        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            token.ThrowIfCancellationRequested();

            if (attribute.AttributeClass?.ToString() != "ReactiveMarbles.ObservableEvents.GenerateStaticEventObservablesAttribute")
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol type)
            {
                continue;
            }

            var location = attribute.ApplicationSyntaxReference == null ? Location.None : Location.Create(attribute.ApplicationSyntaxReference.SyntaxTree, attribute.ApplicationSyntaxReference.Span);

            staticNamespaceList.Add((location, type));
        }
    }

    private static bool GenerateEvents(
        Compilation compilation,
        IEventSymbolGenerator symbolGenerator,
        bool isStatic,
        IReadOnlyList<(Location Location, INamedTypeSymbol NamedType)> symbols,
        Action<string, string> addSoureText,
        Action<Diagnostic> addDiagnostic,
        List<MethodDeclarationSyntax> methodInvocationExtensions,
        CancellationToken token)
    {
        if (symbols.Count == 0)
        {
            return true;
        }

        var fileType = isStatic ? "Static" : "Instance";

        var rootContainingSymbols = symbols.Select(x => x.NamedType).ToImmutableSortedSet(TypeDefinitionNameComparer.Default);

        bool hasEvents = false;

        foreach (var item in rootContainingSymbols)
        {
            token.ThrowIfCancellationRequested();
            var namespaceItem = symbolGenerator.Generate(item, compilation.GetTypeByMetadataName);

            if (namespaceItem == null)
            {
                continue;
            }

            hasEvents = true;

            var compilationUnit = GenerateCompilationUnit(namespaceItem);

            if (compilationUnit == null)
            {
                continue;
            }

            var sourceText = compilationUnit.ToFullString();

            SymbolDisplayFormat fileNameFormat = RoslynHelpers.SymbolDisplayFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None);
            string genericHint = item.IsGenericType ? $"-{string.Join("-", item.TypeParameters.Select(type => type.Name))}" : string.Empty;
            var name = $"SourceClass{item.ToDisplayString(fileNameFormat)}{genericHint}-{fileType}Events.SourceGenerated.cs";

            addSoureText.Invoke(
                name,
                sourceText);

            methodInvocationExtensions?.Add(item.GenerateMethod());
        }

        if (!hasEvents)
        {
            addDiagnostic.Invoke(Diagnostic.Create(DiagnosticWarnings.EventsNotFound, symbols.First().Location));
        }

        return true;
    }

    private static CompilationUnitSyntax? GenerateCompilationUnit(params NamespaceDeclarationSyntax?[] namespaceItems)
    {
        var members = new List<MemberDeclarationSyntax>(namespaceItems.Length);
        for (int i = 0; i < namespaceItems.Length; ++i)
        {
            var namespaceItem = namespaceItems[i];

            if (namespaceItem == null)
            {
                continue;
            }

            members.Add(namespaceItem);
        }

        if (members.Count == 0)
        {
            return null;
        }

        return CompilationUnit(default, members, default)
            .WithLeadingTrivia(
                XmlSyntaxFactory.GenerateDocumentationString(
                    "<auto-generated />"));
    }
}
