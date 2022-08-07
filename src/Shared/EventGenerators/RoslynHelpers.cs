// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators
{
    /// <summary>
    /// Helper methods associated with the roslyn template generators.
    /// </summary>
    internal static class RoslynHelpers
    {
        internal const string ObservableUnitName = "global::System.Reactive.Unit";
        internal const string VoidType = "System.Void";

        public static SymbolDisplayFormat SymbolDisplayFormat { get; } = new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance);

        public static SymbolDisplayFormat TypeFormat { get; } = new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        /// <summary>
        /// Gets an argument which access System.Reactive.Unit.Default member.
        /// </summary>
        public static ArgumentSyntax[] ReactiveUnitArgumentList { get; } = new[] { Argument(ObservableUnitName + ".Default") };

        public static Func<INamedTypeSymbol, bool> HasEvents { get; } = symbol => symbol.GetMembers().OfType<IEventSymbol>().Any(x => x.DeclaredAccessibility == Accessibility.Public);

        /// <summary>
        /// Gets information about the event's obsolete information if any.
        /// </summary>
        /// <param name="eventDetails">The event details.</param>
        /// <returns>The event's obsolete information if there is any.</returns>
        public static AttributeListSyntax[] GenerateObsoleteAttributeList(ISymbol eventDetails)
        {
            var obsoleteAttribute = eventDetails.GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.GetArityDisplayName().Equals("System.ObsoleteAttribute", StringComparison.InvariantCulture) ?? false);

            if (obsoleteAttribute == null)
            {
                return Array.Empty<AttributeListSyntax>();
            }

            var message = obsoleteAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
            var isError = bool.Parse(obsoleteAttribute.ConstructorArguments.ElementAtOrDefault(1).Value?.ToString() ?? bool.FalseString) ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;

            if (message != null && !string.IsNullOrWhiteSpace(message))
            {
                var attribute = Attribute(
                    "global::System.ObsoleteAttribute",
                    new[] { AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(message))), AttributeArgument(LiteralExpression(isError)) });
                return new[] { AttributeList(attribute) };
            }

            return Array.Empty<AttributeListSyntax>();
        }
    }
}
