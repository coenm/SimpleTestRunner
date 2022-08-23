namespace Publisher
{
    using System;
    using AutoMapper;
    using Interface;
    using NetMQ.Sockets;
    using Interface.Data.Logger;
    using NetMQ;
    using Newtonsoft.Json;

    public class Publisher : IDisposable
    {
        private readonly PublisherSocket _pubSocket;
        private readonly IMapper _mapper;
        private readonly string _session = Guid.NewGuid().ToString();

        public Publisher()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
            _mapper = config.CreateMapper();

            _pubSocket = new PublisherSocket();
            _pubSocket.Options.SendHighWatermark = 1000;
        }

        public void Start(int port)
        {
            _pubSocket.Connect($"tcp://localhost:{port}");
        }

        public void Send(EventArgs evt, string caller)
        {
            var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
            dto!.SessionId = _session;
            var json = JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
            var @type = dto.GetType().Name;
            Console.WriteLine($"Send {@type}");
            Console.WriteLine(json);

            _pubSocket
                .SendMoreFrame("DataCollector") // Logger
                .SendMoreFrame(caller)
                .SendMoreFrame(@type)
                .SendFrame(json);
        }

        public void Dispose()
        {
            // too soon??
            _pubSocket.Dispose();
        }
    }
}