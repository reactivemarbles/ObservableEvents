// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    /// <summary>
    /// Generates common code generation between both static and instance based observables for events.
    /// </summary>
    internal abstract class EventGeneratorBase : IEventSymbolGenerator
    {
        /// <inheritdoc />
        public abstract NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item, bool generateEmpty);

        /// <summary>
        /// Generates an observable declaration that wraps a event.
        /// </summary>
        /// <param name="eventDetails">The details of the event to wrap.</param>
        /// <param name="dataObjectName">The name of the item where the event is stored.</param>
        /// <param name="prefix">A prefix to append to the name.</param>
        /// <returns>The property declaration.</returns>
        protected static PropertyDeclarationSyntax? GenerateEventWrapperObservable(IEventSymbol eventDetails, string dataObjectName, string? prefix)
        {
            prefix ??= string.Empty;

            // Create "Observable.FromEvent" for our method.
            var (expressionBody, observableEventArgType) = GenerateFromEventExpression(eventDetails, dataObjectName);

            if (observableEventArgType == null || expressionBody == null)
            {
                return null;
            }

            var modifiers = eventDetails.IsStatic
                ? TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                : TokenList(Token(SyntaxKind.PublicKeyword));

            // Produces for static: public static global::System.IObservable<(argType1, argType2)> EventName => (contents of expression body)
            // Produces for instance: public global::System.IObservable<(argType1, argType2)> EventName => (contents of expression body)
            return PropertyDeclaration(observableEventArgType, prefix + eventDetails.Name)
                .WithModifiers(modifiers)
                .WithExpressionBody(expressionBody)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .WithObsoleteAttribute(eventDetails)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Gets an observable which signals when the {0} event triggers.", eventDetails.ConvertToDocument()));
        }

        private static (ArrowExpressionClauseSyntax ArrowClause, TypeSyntax EventArgsType) GenerateFromEventExpression(IEventSymbol eventSymbol, string dataObjectName)
        {
            ArgumentListSyntax methodParametersArgumentList;
            TypeSyntax eventArgsType;

            var invokeMethod = ((INamedTypeSymbol)eventSymbol.OriginalDefinition.Type).DelegateInvokeMethod;

            if (invokeMethod == null)
            {
                return default;
            }

            var returnType = IdentifierName(eventSymbol.Type.GenerateFullGenericName());

            // If we are using a standard approach of using 2 parameters only send the "Value", not the sender.
            if (invokeMethod.Parameters.Length == 2 && invokeMethod.Parameters[0].Type.GenerateFullGenericName() == "object")
            {
                methodParametersArgumentList = invokeMethod.Parameters[1].GenerateArgumentList();
                eventArgsType = IdentifierName(invokeMethod.Parameters[1].Type.GenerateFullGenericName());
            }
            else if (invokeMethod.Parameters.Length > 0)
            {
                // If we have any members call our observables with the parameters.
                // If we have only one member, produces arguments: (arg1);
                // If we have greater than one member, produces arguments with value type: ((arg1, arg2))
                methodParametersArgumentList = invokeMethod.Parameters.Length == 1 ? invokeMethod.Parameters[0].GenerateArgumentList() : invokeMethod.Parameters.GenerateTupleArgumentList();
                eventArgsType = invokeMethod.Parameters.Length == 1 ? IdentifierName(invokeMethod.Parameters[0].Type.GenerateFullGenericName()) : invokeMethod.Parameters.Select(x => (x.Type, x.Name)).GenerateTupleType();
            }
            else
            {
                // Produces argument: (global::System.Reactive.Unit.Default)
                methodParametersArgumentList = RoslynHelpers.ReactiveUnitArgumentList;
                eventArgsType = IdentifierName(RoslynHelpers.ObservableUnitName);
            }

            var eventName = eventSymbol.Name;

            // Produces local function: void Handler(DataType1 eventParam1, DataType2 eventParam2) => eventHandler(eventParam1, eventParam2)
            var localFunctionExpression = LocalFunctionStatement(
                                                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                                                Identifier("Handler"))
                                            .WithParameterList(invokeMethod.GenerateMethodParameters())
                                            .WithExpressionBody(
                                                ArrowExpressionClause(
                                                    InvocationExpression(IdentifierName("eventHandler"))
                                                        .WithArgumentList(methodParametersArgumentList)))
                                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

            // Produces lambda expression: eventHandler => (local function above); return Handler;
            var conversionLambdaExpression = SimpleLambdaExpression(
                Parameter(Identifier("eventHandler")),
                Block(localFunctionExpression, ReturnStatement(IdentifierName("Handler"))));

            // Produces type parameters: <EventArg1Type, EventArg2Type>
            var fromEventTypeParameters = TypeArgumentList(SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { returnType, Token(SyntaxKind.CommaToken), eventArgsType }));

            // Produces: => global::System.Reactive.Linq.Observable.FromEvent<TypeParameters>(h => (handler from above), x => x += DataObject.Event, x => x -= DataObject.Event);
            var expression = ArrowExpressionClause(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("global::System.Reactive.Linq.Observable"),
                        GenericName(Identifier("FromEvent"))
                            .WithTypeArgumentList(fromEventTypeParameters)))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]
                                    {
                                            Argument(conversionLambdaExpression),
                                            Token(SyntaxKind.CommaToken),
                                            GenerateArgumentEventAccessor(SyntaxKind.AddAssignmentExpression, eventName, dataObjectName),
                                            Token(SyntaxKind.CommaToken),
                                            GenerateArgumentEventAccessor(SyntaxKind.SubtractAssignmentExpression, eventName, dataObjectName)
                                    }))));

            return (expression, eventArgsType.GenerateObservableType());
        }

        private static ArgumentSyntax GenerateArgumentEventAccessor(SyntaxKind accessor, string eventName, string dataObjectName)
        {
            // This produces "x => dataObject.EventName += x" and also "x => dataObject.EventName -= x" depending on the accessor passed in.
            return Argument(
                SimpleLambdaExpression(
                    Parameter(Identifier("x")),
                    AssignmentExpression(
                        accessor,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(dataObjectName),
                            IdentifierName(eventName)),
                        IdentifierName("x"))));
        }
    }
}
