namespace TestRunner.Core;

using System;
using System.Collections.ObjectModel;

public class EventCollection : Collection<string>
{
    public event EventHandler<string> LineAdded = delegate { };

    protected override void InsertItem(int index, string item)
    {
        base.InsertItem(index, item);
        LineAdded.Invoke(this, item);
    }
}