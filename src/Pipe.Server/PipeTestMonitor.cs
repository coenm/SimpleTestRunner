namespace Pipe.Server
{
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
            // _server.ClientConnected += async (o, args) =>
            //     {
            //         // Clients.Add(args.Connection.PipeName);
            //         // UpdateClientList();
            //
            //         // AddLine($"{args.Connection.PipeName} connected!");
            //
            //         // try
            //         // {
            //         //     await args.Connection.WriteAsync("Welcome! You are now connected to the server.").ConfigureAwait(false);
            //         // }
            //         // catch (Exception exception)
            //         // {
            //         //     OnExceptionOccurred(exception);
            //         // }
            //     };
            // _server.ClientDisconnected += (o, args) =>
            //     {
            //         Clients.Remove(args.Connection.PipeName);
            //         UpdateClientList();
            //
            //         AddLine($"{args.Connection.PipeName} disconnected!");
            //     };
            _server.MessageReceived += ServerOnMessageReceived;
            _ = _server.StartAsync();
            Task.Factory.StartNew((state) =>
                {
                    while (true)
                    {
                        if (_messages.TryDequeue(out EventArgsBaseDto? x))
                        {
                            _subject.OnNext(x);
                        }
                    }

                }, TaskCreationOptions.LongRunning);
            // _server.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);
        }

        private void ServerOnMessageReceived(object? sender, ConnectionMessageEventArgs<string?> e)
        {
            // AddLine($"{args.Connection.PipeName}: {args.Message}");
            var result = _serializer.Deserialize(e.Message);
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
}