namespace Pipe.Publisher;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using H.Pipes;
using Interface;
using Interface.Data.Logger;
using Serialization;

public class Publisher : IDisposable
{
    private readonly IPublisherOutput _output;
    private readonly IMapper? _mapper;
    private readonly Serialization _s;
    private readonly PipeClient<string> _publisher;
    private readonly List<Task> _tasks = new();

    public Publisher(IPublisherOutput output, string pipeName)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        // try
        // {
        //     var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
        //     _mapper = config.CreateMapper();
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e);
        // }
        _s = new Serialization();
        _publisher = new PipeClient<string>(pipeName);
        _publisher.AutoReconnect = true;
        _ = _publisher.ConnectAsync();
    }
    
    public void Send(EventArgs evt, string caller)
    {
        if (_mapper?.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) is not EventArgsBaseDto dto)
        {
            _output.Write($"------------------ {caller} ------------------------" + Environment.NewLine);
            return;
        }


        var s = _s.Serialize(dto);
        try
        {
            _tasks.Add(_publisher.WriteAsync(s, CancellationToken.None));
        }
        catch (Exception e)
        {
            _output.Write($"Could not write dto to pipe. {e.Message}{Environment.NewLine}");
            _output.Write(s + Environment.NewLine);
        }

        // Send(dto);
    }

    public void Send(EventArgsBaseDto dto)
    {
        var s = _s.Serialize(dto);
        try
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine(s);
            Console.WriteLine("----------------------------------");
            _tasks.Add(_publisher.WriteAsync(s, CancellationToken.None));
        }
        catch (Exception e)
        {
            _output.Write($"Could not write dto to pipe. {e.Message}{Environment.NewLine}");
            _output.Write(s + Environment.NewLine);
        }
    }

    private async Task WaitOrCancel(CancellationToken ct)
    {
        try
        {
            await Task.WhenAll(_tasks).WithWaitCancellation(ct).ConfigureAwait(false);
        }
        catch (Exception)
        {
            _output.Write("Not all test events are published." + Environment.NewLine);
        }
    }
    
    public void Dispose()
    {
        WaitOrCancel(new CancellationTokenSource(5000).Token).GetAwaiter().GetResult();
        _publisher.DisposeAsync().GetAwaiter().GetResult();
    }
}