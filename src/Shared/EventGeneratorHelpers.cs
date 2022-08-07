// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators;
using ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators;
using ReactiveMarbles.PropertyChanged.SourceGenerator;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator;

/// <summary>
/// Generates Observables from events in specified types and namespaces.
/// </summary>
internal static class EventGeneratorHelpers
{
    private static readonly InstanceEventGenerator _eventGenerator = new();
    private static readonly StaticEventGenerator _staticEventGenerator = new();

    public static void ExtractEvents(Action<string, string> addText, Action<Diagnostic> addDiagnostic, Compilation compilation, IEnumerable<InvocationExpressionSyntax> events)
    {
        var extensionMethodInvocations = new List<MethodDeclarationSyntax>();
        var staticMethodInvocations = new List<MethodDeclarationSyntax>();

        GetAvailableTypes(compilation, events, out var instanceNamespaceList, out var staticNamespaceList);

        GenerateEvents(addDiagnostic, addText, _staticEventGenerator, true, staticNamespaceList, staticMethodInvocations);
        GenerateEvents(addDiagnostic, addText, _eventGenerator, false, instanceNamespaceList, extensionMethodInvocations);

        GenerateEventExtensionMethods(addText, extensionMethodInvocations);
    }

    private static void GenerateEventExtensionMethods(
        Action<string, string> addSource,
        List<MethodDeclarationSyntax> methodInvocationExtensions)
    {
        var classDeclaration = ClassDeclaration("ObservableGeneratorExtensions", new[] { SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword, SyntaxKind.PartialKeyword }, methodInvocationExtensions, 1);

        var compilationUnit = GenerateCompilationUnit(classDeclaration);

        if (compilationUnit == null)
        {
            return;
        }

        addSource("TestExtensions.FoundEvents.SourceGenerated.cs", compilationUnit.ToFullString());
    }

    private static void GetAvailableTypes(
        Compilation compilation,
        IEnumerable<InvocationExpressionSyntax> eventInvocations,
        out List<(Location Location, INamedTypeSymbol NamedType)> instanceNamespaceList,
        out List<(Location Location, INamedTypeSymbol NamedType)> staticNamespaceList)
    {
        var observableGeneratorExtensions = compilation.GetTypeByMetadataName("ObservableGeneratorExtensions");

        if (observableGeneratorExtensions == null)
        {
            throw new InvalidOperationException("Cannot find ObservableGeneratorExtensions");
        }

        instanceNamespaceList = new List<(Location Location, INamedTypeSymbol NamedType)>();
        staticNamespaceList = new List<(Location Location, INamedTypeSymbol NamedType)>();

        foreach (var invocation in eventInvocations)
        {
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

            var location = Location.Create(invocation.SyntaxTree, invocation.Span);

            instanceNamespaceList.Add((location, callingSymbol));
        }

        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() != "GenerateStaticEventObservablesAttribute")
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
        Action<Diagnostic> reportDiagnostic,
        Action<string, string> addSource,
        IEventSymbolGenerator symbolGenerator,
        bool isStatic,
        IReadOnlyList<(Location Location, INamedTypeSymbol NamedType)> symbols,
        List<MethodDeclarationSyntax>? methodInvocationExtensions = null)
    {
        if (symbols.Count == 0)
        {
            return true;
        }

        var processedItems = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        var fileType = isStatic ? "Static" : "Instance";

        var rootContainingSymbols = symbols.Select(x => x.NamedType).ToImmutableSortedSet(TypeDefinitionNameComparer.Default);

        bool hasEvents = false;

        foreach (var (location, item) in symbols)
        {
            if (processedItems.Contains(item))
            {
                continue;
            }

            processedItems.Add(item);

            var namespaceItem = symbolGenerator.Generate(item);

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

            var name = $"SourceClass{item.ToDisplayString(RoslynHelpers.SymbolDisplayFormat)}-{fileType}Events.SourceGenerated.cs";

            addSource(name, sourceText);

            methodInvocationExtensions?.Add(item.GenerateMethod());
        }

        if (!hasEvents)
        {
            reportDiagnostic(Diagnostic.Create(DiagnosticWarnings.EventsNotFound, symbols[0].Location));
            return false;
        }

        return true;
    }

    private static CompilationUnitSyntax? GenerateCompilationUnit(params MemberDeclarationSyntax?[] members)
    {
        var membersList = new List<MemberDeclarationSyntax>(members.Length);
        for (int i = 0; i < members.Length; ++i)
        {
            var member = members[i];

            if (member == null)
            {
                continue;
            }

            membersList.Add(member);
        }

        if (membersList.Count == 0)
        {
            return null;
        }

        return CompilationUnit(default, membersList, default)
            .WithLeadingTrivia(
                XmlSyntaxFactory.GenerateDocumentationString(
                    "<auto-generated />"));
    }
}
