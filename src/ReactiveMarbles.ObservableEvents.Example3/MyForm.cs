// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

using Xamarin.Forms;

[assembly: GenerateStaticEventObservables(typeof(ReactiveMarbles.ObservableEvents.Example.StaticTest))]

#pragma warning disable
namespace ReactiveMarbles.ObservableEvents.Example;

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

        RxStaticTestEvents.TestChanged.Subscribe(); 
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
