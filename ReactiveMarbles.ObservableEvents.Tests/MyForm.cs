using System;
using System.Collections.Generic;
using System.Text;

using ReactiveMarbles.ObservableEvents;

using Xamarin.Forms;

[assembly: TypeEventsToObservables(typeof(Page))]

namespace ReactiveMarbles.ObservableEvents
{
    [TypeEventsToObservables]
    public partial class MyForm : Page
    {
        public void Test()
        {
            this.Events().PropertyChanged.Subscribe(x => Console.WriteLine(x));
        }
    }
}
