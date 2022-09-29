namespace NetMq.Publisher;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using H.Pipes;
using Interface;
using Interface.Data.Logger;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Serialization;

public class Publisher : IDisposable
{
    private readonly IOutput _output;
    private readonly IMapper _mapper;
    private readonly Serialization _s;
    private readonly PipeClient<string> _publisher;
    private List<Task> _tasks = new List<Task>();

    public Publisher(IOutput output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
        _mapper = config.CreateMapper();
        _s = new Serialization();
        _publisher = new PipeClient<string>("named_pipe_test_server");
        _publisher.AutoReconnect = true;
        _ = _publisher.ConnectAsync();
    }
    
    public void Send(EventArgs evt, string caller)
    {
        if (_mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) is EventArgsBaseDto dto)
        {
            // var serialize = _s.Serialize(dto);
            // Console.WriteLine(serialize);
            // Console.SetError(new StreamWriter(Console.OpenStandardError()));
            // Console.Error.WriteLine(serialize);
            // Console.Error.Flush();

            var s = _s.Serialize(dto);
            try
            {
                _tasks.Add(_publisher.WriteAsync(s, CancellationToken.None));
                // _publisher.WriteAsync(s, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not write dto to hpipe. {e.Message}");
                _output.Write(s + Environment.NewLine, OutputLevel.Information);
            }
        }
    }

    public void Dispose()
    {
        Task.WhenAll(_tasks).GetAwaiter().GetResult();
        _publisher.DisposeAsync().GetAwaiter().GetResult();
    }
}