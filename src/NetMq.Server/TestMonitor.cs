namespace NetMq.Server;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Interface.Data.Logger;
using Interface.Server;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

public class TestMonitor : ITestMonitor, IDisposable
{
    private static readonly List<Type> _types = typeof(EventArgsBaseDto).Assembly
                                                                        .GetTypes()
                                                                        .Where(t => t.IsSubclassOf(typeof(EventArgsBaseDto)))
                                                                        .ToList();
    private SubscriberSocket _subSocket;
    private readonly ReplaySubject<EventArgsBaseDto> _subject;

    public TestMonitor()
    {
        var window = TimeSpan.FromSeconds(5);
        _subject = new ReplaySubject<EventArgsBaseDto>(window);
    }

    public IObservable<EventArgsBaseDto> Events => _subject;

    public Task StartMonitoring(int port)
    {
        StopMonitoring();
        _subSocket = new SubscriberSocket();
        _subSocket.Options.ReceiveHighWatermark = 1000;
        _subSocket.Bind($"tcp://*:{port}");
        _subSocket.Subscribe(string.Empty); // all.
        Console.WriteLine("Subscriber socket connecting...");

        // long running, fix
        return Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        var incomingTopic = _subSocket.ReceiveFrameString();
                        var eventName = _subSocket.ReceiveFrameString();
                        var msgType = _subSocket.ReceiveFrameString();
                        var payload = _subSocket.ReceiveFrameString();

                        EventArgsBaseDto? evt = null;
                        try
                        {
                            Type @type = _types.Single(x => x.Name.Equals(msgType));
                            evt = JsonConvert.DeserializeObject(payload, @type) as EventArgsBaseDto;
                        }
                        catch (Exception e)
                        {
                            _subject.OnError(new Exception($"Cannot parse incoming json to type, {e.Message}", e));
                        }

                        if (evt != null)
                        {
                            // _subject.OnNext(o);
                            Task.Run(() => _subject.OnNext(evt));
                        }
                        else
                        {
                            _subject.OnError(new Exception($"Cannot parse incoming json to type."));
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // throw;
                }
            },
            TaskCreationOptions.LongRunning);
    }

    public void StopMonitoring()
    {
        _subSocket?.Dispose();
    }

    public void Dispose()
    {
        _subSocket?.Dispose();
        _subject.Dispose();
    }
}