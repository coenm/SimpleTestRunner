namespace TestRunner.Core;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Interface.Data.Logger;
using Interface.Server;
using Serialization;

public class EvtMonitor : ITestMonitor, IDisposable
{
    private readonly ConsoleOutputProcessor _outputProcessor;
    private readonly ReplaySubject<EventArgsBaseDto> _subject;

    public EvtMonitor(ConsoleOutputProcessor outputProcessor)
    {
        _outputProcessor = outputProcessor ?? throw new ArgumentNullException(nameof(outputProcessor));
        var window = TimeSpan.FromSeconds(5);
        _subject = new ReplaySubject<EventArgsBaseDto>(window);
        _outputProcessor.OnEvent += OutputProcessorOnOnEvent;
    }

    private void OutputProcessorOnOnEvent(object? sender, EventArgsBaseDto e)
    {
        _subject.OnNext(e);
    }

    public IObservable<EventArgsBaseDto> Events => _subject;

    public void Dispose()
    {
        _outputProcessor.OnEvent -= OutputProcessorOnOnEvent;
    }
}

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