// Copyright (c) 2019-2021 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

using ReactiveMarbles.ObservableEvents;

using Xamarin.Forms;

[assembly: GenerateStaticEventObservables(typeof(StaticTest))]

#pragma warning disable
namespace ReactiveMarbles.ObservableEvents
{
    /// <summary>
    /// Code behind for the same page.
    /// </summary>
    public partial class MyForm : Page
    {
        /// <summary>
        /// Test command.
        /// </summary>
        public void Test()
        {
            this.Events().PropertyChanged.Subscribe();

            ////RxStaticTestEvents.TestChanged.Subscribe(); 
        }
    }

    public class MyNoEventsClass
    {
        public MyNoEventsClass()
        {
            this.Events();
        }
    }

    public class AsyncEventsClass
    {
        public event Func<int, Task> TestEvent1;
        public event Func<int, ValueTask> TestEvent2;

        public AsyncEventsClass()
        {
            var e1 = this.Events().TestEvent1;
            var e2 = this.Events().TestEvent2;
        }
    }

    public static class StaticTest
    {
        public static event EventHandler? TestChanged;
    }

    public class GenericTestClass<TGenericType>
    {
        public event EventHandler<TGenericType> TestChanged;

        public GenericTestClass()
        {
            var events = this.Events().TestChanged;
        }
    }
}
