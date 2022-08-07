// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

using static ReactiveMarbles.ObservableEvents.SourceGenerator.SyntaxFactoryHelpers;

namespace ReactiveMarbles.ObservableEvents.SourceGenerator
{
    internal static class SyntaxExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddLeadingNewLines(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithLeadingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddTrialingNewLines(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithTrailingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddLeadingNewLinesAndSpaces(this SyntaxToken item, int numberNewLines = 1, int numberSpaces = 1)
        {
            if (numberNewLines == 0 && numberSpaces == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, numberNewLines);
            var leadingSpaces = Enumerable.Repeat(Space, numberSpaces);

            return item.WithLeadingTrivia(carriageReturnList.Concat(leadingSpaces));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddLeadingSpaces(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithLeadingTrivia(leadingSpaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SyntaxToken AddTrialingSpaces(this SyntaxToken item, int number = 1)
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithTrailingTrivia(leadingSpaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddLeadingNewLinesAndSpaces<T>(this T item, int numberNewLines = 1, int numberSpaces = 1)
            where T : SyntaxNode
        {
            if (numberNewLines == 0 && numberSpaces == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, numberNewLines);
            var leadingSpaces = Enumerable.Repeat(Space, numberSpaces);

            return item.WithLeadingTrivia(carriageReturnList.Concat(leadingSpaces));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddLeadingNewLines<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithLeadingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddTrialingNewLines<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var carriageReturnList = Enumerable.Repeat(CarriageReturnLineFeed, number);
            return item.WithTrailingTrivia(carriageReturnList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddLeadingSpaces<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithLeadingTrivia(leadingSpaces);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AddTrialingSpaces<T>(this T item, int number = 1)
            where T : SyntaxNode
        {
            if (number == 0)
            {
                return item;
            }

            var leadingSpaces = Enumerable.Repeat(Space, number);
            return item.WithTrailingTrivia(leadingSpaces);
        }
    }
}
