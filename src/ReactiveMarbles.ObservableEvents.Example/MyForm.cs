// Copyright (c) 2019-2021 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

using ReactiveMarbles.ObservableEvents;

using Xamarin.Forms;

[assembly: GenerateStaticEventObservablesAttribute(typeof(StaticTest))]

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

    public static class StaticTest
    {
        public static event EventHandler? TestChanged;
    }
}
