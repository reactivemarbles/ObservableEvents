// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator.EventGenerators.Generators
{
    /// <summary>
    /// Generates common code generation between both static and instance based observables for events.
    /// </summary>
    internal abstract class EventGeneratorBase : IEventSymbolGenerator
    {
        /// <inheritdoc />
        public abstract NamespaceDeclarationSyntax? Generate(INamedTypeSymbol item);

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
                ? new[] { SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword }
                : new[] { SyntaxKind.PublicKeyword };

            var attributes = RoslynHelpers.GenerateObsoleteAttributeList(eventDetails);

            // Produces for static: public static global::System.IObservable<(argType1, argType2)> EventName => (contents of expression body)
            // Produces for instance: public global::System.IObservable<(argType1, argType2)> EventName => (contents of expression body)
            return PropertyDeclaration(observableEventArgType, prefix + eventDetails.Name, attributes, modifiers, expressionBody, 2)
                .WithLeadingTrivia(XmlSyntaxFactory.GenerateSummarySeeAlsoComment("Gets an observable which signals when the {0} event triggers.", eventDetails.ConvertToDocument()))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static (ArrowExpressionClauseSyntax ArrowClause, TypeSyntax EventArgsType) GenerateFromEventExpression(IEventSymbol eventSymbol, string dataObjectName)
        {
            IReadOnlyCollection<ArgumentSyntax> methodParametersArgumentList;
            TypeSyntax eventArgsType;

            var invokeMethod = ((INamedTypeSymbol)eventSymbol.Type).DelegateInvokeMethod;

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
                eventArgsType = invokeMethod.Parameters.Length == 1 ? IdentifierName(invokeMethod.Parameters[0].Type.GenerateFullGenericName()) : invokeMethod.Parameters.Select(x => (x.Type, x.Name)).ToList().GenerateTupleType();
            }
            else
            {
                // Produces argument: (global::System.Reactive.Unit.Default)
                methodParametersArgumentList = RoslynHelpers.ReactiveUnitArgumentList;
                eventArgsType = IdentifierName(RoslynHelpers.ObservableUnitName);
            }

            var eventName = eventSymbol.Name;

            var localFunctionExpression = GenerateLocalHandler(methodParametersArgumentList, invokeMethod, "obs.OnNext");

            ////// Produces lambda expression: eventHandler => (local function above); return Handler;
            ////var conversionLambdaExpression = SimpleLambdaExpression(Parameter("eventHandler"), Block(new StatementSyntax[] { localFunctionExpression, ReturnStatement("Handler") }, 3));

            // Produces type parameters: <EventArg1Type, EventArg2Type>
            var fromEventTypeParameters = new[] { returnType, eventArgsType };

            var conversionLambdaExpression = SimpleLambdaExpression(Parameter("obs"), Block(
                new StatementSyntax[]
                {
                    localFunctionExpression,
                    ExpressionStatement(GenerateEventAssignment(SyntaxKind.AddAssignmentExpression, eventName, dataObjectName, "Handler")),
                    ReturnStatement(InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "global::System.Reactive.Disposables.Disposable", "Create"),
                        new ArgumentSyntax[]
                        {
                            Argument(ParenthesizedLambdaExpression(GenerateEventAssignment(SyntaxKind.SubtractAssignmentExpression, eventName, dataObjectName, "Handler")))
                        })),
                },
                3));

            // Produces: => global::System.Reactive.Linq.Observable.Create<TypeParameter>(obs =>
            // {
            //    void Handler(...event params...) => obs.OnNext(params);
            //    _data.Event += Handler;
            //    return global::System.Reactive.Disposables.Disposable.Create(() => _data.Event -= Handler);
            // }
            var expression = ArrowExpressionClause(InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, "global::System.Reactive.Linq.Observable", GenericName("Create", new[] { eventArgsType })),
                new[]
                {
                    Argument(conversionLambdaExpression),
                }));

            return (expression, eventArgsType.GenerateObservableType());
        }

        private static LocalFunctionStatementSyntax GenerateLocalHandler(IReadOnlyCollection<ArgumentSyntax> methodParametersArgumentList, IMethodSymbol invokeMethod, string handlerName) =>

            // Produces local function: void Handler(DataType1 eventParam1, DataType2 eventParam2) => eventHandler(eventParam1, eventParam2)
            LocalFunctionStatement(
                "void",
                "Handler",
                invokeMethod.GenerateMethodParameters(),
                ArrowExpressionClause(
                        InvocationExpression(handlerName, methodParametersArgumentList)));

        private static AssignmentExpressionSyntax GenerateEventAssignment(SyntaxKind accessor, string eventName, string dataObjectName, string handlerName)
        {
            // This produces "x => dataObject.EventName += x" and also "x => dataObject.EventName -= x" depending on the accessor passed in.
            return AssignmentExpression(
                        accessor,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            dataObjectName,
                            eventName),
                        handlerName);
        }
    }
}
