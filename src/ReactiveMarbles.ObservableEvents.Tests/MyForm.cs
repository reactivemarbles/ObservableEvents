// Copyright (c) 2020 ReactiveUI Association Inc. All rights reserved.
// ReactiveUI Association Inc licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

using ReactiveMarbles.ObservableEvents;

using Xamarin.Forms;

namespace ReactiveMarbles.ObservableEvents
{
    /// <summary>
    /// Code behind for the same page.
    /// </summary>
    [TypeEventsToObservables]
    public partial class MyForm : Page
    {
        /// <summary>
        /// Test command.
        /// </summary>
        public void Test()
        {
            this.Events().PropertyChanged.Subscribe(x => Console.WriteLine(x));
        }
    }
}
