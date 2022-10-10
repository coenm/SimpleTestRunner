namespace Pipe.Server;

using System.Collections.Concurrent;
using System.Reactive.Subjects;
using H.Pipes;
using H.Pipes.Args;
using Interface.Data.Logger;
using Interface.Server;
using Serialization;

public class PipeTestMonitor : ITestMonitor, IAsyncDisposable
{
    private readonly PipeServer<string> _server;
    private readonly ReplaySubject<EventArgsBaseDto> _subject;
    private readonly Serialization _serializer;
    private readonly ConcurrentQueue<EventArgsBaseDto> _messages;
        
    public IObservable<EventArgsBaseDto> Events => _subject;

    public PipeTestMonitor(string pipeName)
    {
        _messages = new ConcurrentQueue<EventArgsBaseDto>();
        _serializer = new Serialization();
        var window = TimeSpan.FromSeconds(5);
        _subject = new ReplaySubject<EventArgsBaseDto>(window);

        _server = new PipeServer<string>(pipeName);
        _server.ClientConnected += (o, args) =>
            {
                Console.WriteLine("connected");
            };
        _server.ClientDisconnected += (o, args) =>
            {
                Console.WriteLine("disconnected");
            };

        _server.MessageReceived += ServerOnMessageReceived;
        _server.StartAsync();

        Task.Factory.StartNew(state =>
            {
                while (true)
                {
                    if (_messages.TryDequeue(out EventArgsBaseDto? item))
                    {
                        _subject.OnNext(item);
                    }
                }
            },
            TaskCreationOptions.LongRunning);

        _server.ExceptionOccurred += (o, args) =>
            {
                Console.WriteLine(args.Exception.Message);
            };
    }

    private void ServerOnMessageReceived(object? sender, ConnectionMessageEventArgs<string?> e)
    {
        EventArgsBaseDto? result = _serializer.Deserialize(e.Message);
        if (result != null)
        {
            _messages.Enqueue(result);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _server.DisposeAsync().ConfigureAwait(false);
    }
}