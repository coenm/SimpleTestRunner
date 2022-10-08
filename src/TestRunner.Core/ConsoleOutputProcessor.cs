namespace TestRunner.Core;

using System;
using System.Collections.ObjectModel;

public class ConsoleOutputProcessor : IDisposable
{
    private readonly EventCollection _out;
    private readonly EventCollection _err;
    
    public event EventHandler<string> OnLine = delegate { };
    public event EventHandler<string> OnErr = delegate { };

    public ConsoleOutputProcessor()
    {
        _out = new EventCollection();
        _err = new EventCollection();

        _out.LineAdded += OutOnLineAdded;
        _err.LineAdded += ErrOnLineAdded;
    }

    private void OutOnLineAdded(object? sender, string e)
    {
        OnLine.Invoke(this, e);
    }

    private void ErrOnLineAdded(object? sender, string e)
    {
        OnErr.Invoke(this, e);
    }

    public Collection<string> Out => _out;

    public Collection<string> Err => _err;

    public void Dispose()
    {
        _out.LineAdded -= OutOnLineAdded;
        _err.LineAdded -= ErrOnLineAdded;
    }
}