namespace TestRunner.Core;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Interface.Data.Logger;
using Serialization;

public class ConsoleOutputProcessor : IDisposable
{
    private readonly EventCollection _out;
    private readonly EventCollection _err;
    private readonly Serialization _serializer;
    
    public event EventHandler<string> OnLine = delegate { };

    public event EventHandler<EventArgsBaseDto> OnEvent = delegate { };

    public ConsoleOutputProcessor()
    {
        _serializer = new Serialization();
        _out = new EventCollection();
        _err = new EventCollection();

        _out.LineAdded += OutOnLineAdded;
        _err.LineAdded += ErrOnLineAdded;
    }

    private void OutOnLineAdded(object? sender, string e)
    {
        EventArgsBaseDto? result = _serializer.Deserialize(e);
        if (result == null)
        {
            OnLine.Invoke(this, e);
        }
        else
        {
            OnEvent.Invoke(this, result);
        }
    }

    private void ErrOnLineAdded(object? sender, string e)
    {
        OnLine.Invoke(this, e);
    }

    public Collection<string> Out => _out;

    public Collection<string> Err => _err;

    public void Dispose()
    {
        _out.LineAdded -= OutOnLineAdded;
        _err.LineAdded -= ErrOnLineAdded;
    }
}