// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;

using Xunit;
using Xunit.Abstractions;

namespace ReactiveMarbles.ObservableEvents.Tests
{
    /// <summary>
    /// Tests for generators.
    /// </summary>
    public class GeneratorLibraryTests
    {
        private ITestOutputHelper _testOutputHelper;

        public GeneratorLibraryTests(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [Theory]
        [InlineData(typeof(Xamarin.Forms.WebView))]
        [InlineData(typeof(Avalonia.Controls.CheckBox))]
        [InlineData(typeof(Xamarin.Essentials.Accelerometer))]
        [InlineData(typeof(System.Windows.Input.ICommand))]
        [InlineData(typeof(Uno.UI.Xaml.Controls.ComboBox))]
        public void TestDerived(Type type)
        {
            DeriveEventsTest(type.Assembly.GetTypes());
        }

        [Theory]
        [InlineData(typeof(Xamarin.Forms.WebView))]
        [InlineData(typeof(Avalonia.Controls.CheckBox))]
        [InlineData(typeof(Xamarin.Essentials.Accelerometer))]
        [InlineData(typeof(System.Windows.Input.ICommand))]
        [InlineData(typeof(Uno.UI.Xaml.Controls.ComboBox))]
        public void TestConcrete(Type type)
        {
            ConcreteClassCreate(type.Assembly.GetTypes());
        }

        private static List<System.Reflection.EventInfo> GetValidEvents(Type classDeclaration)
        {
            var list = new List<System.Reflection.EventInfo>();

            foreach (var eventInfo in classDeclaration.GetEvents())
            {
                if (eventInfo.IsObsolete())
                {
                    continue;
                }

                var invokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");

                if (invokeMethod.ReturnType != typeof(void))
                {
                    continue;
                }

                if (eventInfo.GetAddMethod().IsStatic)
                {
                    continue;
                }

                list.Add(eventInfo);
            }

            return list;
        }

        private void DeriveEventsTest(Type[] types)
        {
            foreach (var classDeclaration in types)
            {
                if (!classDeclaration.IsPublic)
                {
                    continue;
                }

                if (classDeclaration.IsSealed)
                {
                    continue;
                }

                if (classDeclaration.IsAbstract)
                {
                    continue;
                }

                if (classDeclaration.IsGenericType)
                {
                    continue;
                }

                if (classDeclaration.IsObsolete())
                {
                    continue;
                }

                if (classDeclaration.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                var eventGenerationCalls = new StringBuilder();

                var events = GetValidEvents(classDeclaration);

                if (events.Count == 0)
                {
                    continue;
                }

                foreach (var eventDeclaration in events)
                {
                    if (eventDeclaration.IsObsolete())
                    {
                        continue;
                    }

                    eventGenerationCalls.Append("          this.Events().").Append(eventDeclaration.Name).AppendLine(".Subscribe();");
                }

                if (eventGenerationCalls.Length == 0)
                {
                    continue;
                }

                var source = $@"
using System;
using System.Collections.Generic;
using System.Text;

using ReactiveMarbles.ObservableEvents;

namespace ReactiveMarbles.ObservableEvents.Tests
{{
    public class TestCode : {classDeclaration.FullName.Replace('+', '.')}
    {{
        public TestCode()
        {{
{eventGenerationCalls}
        }}
    }}
}}
";

                var sourceGeneratorUtility = new SourceGeneratorUtility(_testOutputHelper);
                sourceGeneratorUtility.RunGenerator(
                    types,
                    out _,
                    out _,
                    source);
            }
        }

        private void ConcreteClassCreate(Type[] types)
        {
            foreach (var classDeclaration in types)
            {
                if (!classDeclaration.IsPublic)
                {
                    continue;
                }

                if (classDeclaration.IsAbstract)
                {
                    continue;
                }

                if (classDeclaration.IsGenericType)
                {
                    continue;
                }

                if (classDeclaration.IsObsolete())
                {
                    continue;
                }

                if (classDeclaration.Name != "DependencyObjectCollection")
                {
                    continue;
                }

                if (classDeclaration.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                var eventGenerationCalls = new StringBuilder();

                var events = GetValidEvents(classDeclaration);

                if (events.Count == 0)
                {
                    continue;
                }

                foreach (var eventDeclaration in events)
                {
                    if (eventDeclaration.IsObsolete())
                    {
                        continue;
                    }

                    eventGenerationCalls.Append("          Test.Events().").Append(eventDeclaration.Name).AppendLine(".Subscribe();");
                }

                if (eventGenerationCalls.Length == 0)
                {
                    continue;
                }

                var source = $@"
using System;
using System.Collections.Generic;
using System.Text;

using ReactiveMarbles.ObservableEvents;

namespace ReactiveMarbles.ObservableEvents.Tests
{{
    public class TestCode
    {{
        public {classDeclaration.FullName.Replace('+', '.')} Test {{ get; }} = new();
        public TestCode()
        {{
{eventGenerationCalls}
        }}
    }}
}}
";

                var sourceGeneratorUtility = new SourceGeneratorUtility(_testOutputHelper);
                sourceGeneratorUtility.RunGenerator(
                    types,
                    out _,
                    out _,
                    source);
            }
        }
    }
}
