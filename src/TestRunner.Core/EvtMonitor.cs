namespace TestRunner.Core;

using System;
using System.Reactive.Subjects;
using Interface.Data.Logger;
using Interface.Server;

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