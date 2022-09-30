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
        if (_mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) is not EventArgsBaseDto dto)
        {
            return;
        }

        var s = _s.Serialize(dto);
        try
        {
            _tasks.Add(_publisher.WriteAsync(s, CancellationToken.None));
            // _publisher.WriteAsync(s, CancellationToken.None).GetAwaiter().GetResult();
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

public static class TaskExtensions
{
    /// <summary>
    /// See for more information: 
    /// http://stackoverflow.com/questions/14524209/what-is-the-correct-way-to-cancel-an-async-operation-that-doesnt-accept-a-cance/14524565#14524565
    /// </summary>
    public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Register with the cancellation token.
        await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            // If the task waited on is the cancellation token...
            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                throw new OperationCanceledException(cancellationToken);
        }

        // Wait for one or the other to complete.
        return await task.ConfigureAwait(false);
    }

    public static async Task WithWaitCancellation(this Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Register with the cancellation token.
        await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            // If the task waited on is the cancellation token...
            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                throw new OperationCanceledException(cancellationToken);
        }

        // Wait for one or the other to complete.
        await task.ConfigureAwait(false);
    }
}