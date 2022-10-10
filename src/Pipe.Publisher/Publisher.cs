namespace Pipe.Publisher;

using System;
using System.Collections.Generic;
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
    private readonly List<Task> _tasks = new List<Task>();

    public Publisher(IOutput output, string pipeName)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
        _mapper = config.CreateMapper();
        _s = new Serialization();
        _publisher = new PipeClient<string>(pipeName);
        _publisher.AutoReconnect = true;
        _ = _publisher.ConnectAsync();
    }
    
    public void Send(EventArgs evt, string caller)
    {
        if (_mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) is not EventArgsBaseDto dto)
        {
            return;
        }

        var s = _s.Serialize(dto);
        try
        {
            _tasks.Add(_publisher.WriteAsync(s, CancellationToken.None));
        }
        catch (Exception e)
        {
            _output.Write($"Could not write dto to pipe. {e.Message}{Environment.NewLine}", OutputLevel.Information, ConsoleColor.Red);
            _output.Write(s + Environment.NewLine, OutputLevel.Information, ConsoleColor.Red);
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
            _output.Write("Not all test events are published." + Environment.NewLine, OutputLevel.Information, ConsoleColor.Red);
        }
    }
    
    public void Dispose()
    {
        WaitOrCancel(new CancellationTokenSource(5000).Token).GetAwaiter().GetResult();
        _publisher.DisposeAsync().GetAwaiter().GetResult();
    }
}