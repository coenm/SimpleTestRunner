namespace Publisher
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Interface;
    using NetMQ.Sockets;
    using Interface.Data.Logger;
    using NetMQ;
    using Newtonsoft.Json;

    public class Publisher : IDisposable
    {
        // private readonly PublisherSocket _pubSocket;
        private readonly IMapper _mapper;
        private readonly string _session = Guid.NewGuid().ToString();
        private readonly ConcurrentQueue<NetMQMessage> _q = new ();
        private ManualResetEventSlim _disposeCalled = new(false);
        private ManualResetEventSlim _queueEmpty = new(false);

        public Publisher()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
            _mapper = config.CreateMapper();
        }

        public void Start(int port)
        {
            _ = Task.Factory.StartNew(_ =>
                {
                    using var pubSocket = new PublisherSocket();
                    pubSocket.Options.SendHighWatermark = 1000;
                    pubSocket.Options.Linger = TimeSpan.MinValue;
                    pubSocket.Connect($"tcp://localhost:{port}");

                    while (true)
                    {
                        if (_q.TryDequeue(out NetMQMessage? clientMessage))
                        {
                            for (var i = 0; i < clientMessage.FrameCount -1; i++)
                            {
                                pubSocket.SendMoreFrame(clientMessage[i].ToByteArray());
                            }

                            if (clientMessage.FrameCount > 0)
                            {
                                pubSocket.SendFrame(clientMessage[clientMessage.FrameCount-1].ToByteArray());
                            }
                        }
                        else if (_disposeCalled.IsSet && _q.IsEmpty)
                        {
                            while (pubSocket.HasOut)
                            {
                                Thread.Sleep(500);
                            }

                            _queueEmpty.Set();
                            return;
                        }
                    }
                },
                TaskCreationOptions.LongRunning);
        }

        public void Send(EventArgs evt, string caller)
        {
            var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
            dto!.SessionId = _session;
            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            var @type = dto.GetType().Name;

            var m = new NetMQMessage();
            m.Append("DataCollector"); // Logger
            m.Append(caller);
            m.Append(@type);
            m.Append(json);
            _q.Enqueue(m);
        }

        public void Dispose()
        {
            _disposeCalled.Set();
            _queueEmpty.Wait(TimeSpan.FromSeconds(10));
        }
    }
}