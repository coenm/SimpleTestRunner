// https://github.com/Microsoft/vstest-docs/blob/main/docs/analyze.md
// https://github.com/Microsoft/vstest-docs/blob/main/docs/extensions/datacollector.md
// https://github.com/microsoft/vstest/blob/main/samples/Microsoft.TestPlatform.Protocol/Program.cs
// https://github.com/Microsoft/vstest-docs/blob/main/RFCs/0007-Editors-API-Specification.md

namespace ZmqPublisher.TestLogger;

using System;
using System.Collections.Generic;
using AutoMapper;
using Interface;
using Interface.Data.Logger;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using TestResultEventArgs = Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestResultEventArgs;

[FriendlyName("zmq-test-publisher")]
[ExtensionUri("my://github.com/coenm/test-run-visualizer/zmq-test-publisher")]
public class ZeroMqTestPublisher : ITestLoggerWithParameters
{
    private readonly PublisherSocket _pubSocket;
    private readonly IMapper _mapper;
    private readonly string _session = Guid.NewGuid().ToString();

    public ZeroMqTestPublisher()
    {
        _pubSocket = new PublisherSocket();
        _pubSocket.Options.SendHighWatermark = 10000;

        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
        _mapper = config.CreateMapper();
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        throw new NotImplementedException();
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
    {
        int port = GetPort(parameters);
        _pubSocket.Connect($"tcp://localhost:{port}");

        events.DiscoveryStart += Events_DiscoveryStart;
        events.DiscoveredTests += EventsOnDiscoveredTests;
        events.DiscoveryComplete += EventsOnDiscoveryComplete;
        events.DiscoveryMessage += EventsOnDiscoveryMessage;

        events.TestRunStart += EventsOnTestRunStart;
        events.TestRunComplete += EventsOnTestRunComplete;
        events.TestResult += EventsOnTestResult;
        events.TestRunMessage += EventsOnTestRunMessage;
    }

    private static int GetPort(IReadOnlyDictionary<string, string> configurationElement)
    {
        try
        {
            if (configurationElement.ContainsKey("port"))
            {
                var portValue = configurationElement["port"];
                if (!string.IsNullOrWhiteSpace(portValue))
                {
                    if (int.TryParse(portValue, out var portInt))
                    {
                        if (portInt is > 0 and < int.MaxValue)
                        {
                            return portInt;
                        }
                    }
                }
            }
        }
        catch
        {
            // ignore.
        }

        try
        {
            var portValue = Environment.GetEnvironmentVariable("ZmqLoggerPort");
            if (!string.IsNullOrWhiteSpace(portValue))
            {
                if (int.TryParse(portValue, out var portInt))
                {
                    if (portInt is > 0 and < int.MaxValue)
                    {
                        return portInt;
                    }
                }
            }
        }
        catch (Exception)
        {
            // do nothing
        }

        return 12345;
    }

    private void EventsOnTestRunMessage(object? sender, TestRunMessageEventArgs e)
    {
        Send(e, "TestRunMessage");
    }

    private void EventsOnTestResult(object? sender, TestResultEventArgs e)
    {
        Send(e, "TestResult");
    }

    private void EventsOnTestRunComplete(object? sender, TestRunCompleteEventArgs e)
    {
        Send(e, "TestRunComplete");
    }

    private void EventsOnTestRunStart(object? sender, TestRunStartEventArgs e)
    {
        Send(e, "TestRunStart");
    }

    private void EventsOnDiscoveryMessage(object? sender, TestRunMessageEventArgs e)
    {
        Send(e, "DiscoveryMessage");
    }

    private void EventsOnDiscoveryComplete(object? sender, DiscoveryCompleteEventArgs e)
    {
        Send(e, "DiscoveryComplete");
    }

    private void EventsOnDiscoveredTests(object? sender, DiscoveredTestsEventArgs e)
    {
        Send(e, "DiscoveredTests");
    }

    private void Events_DiscoveryStart(object? sender, DiscoveryStartEventArgs e)
    {
        Send(e, "DiscoveryStart");
    }

    private void Send(EventArgs evt, string caller)
    {
        var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
        dto!.SessionId = _session;
        var json = JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
        var @type = dto.GetType().Name;
        // Console.WriteLine($"Send {@type}");
        // Console.WriteLine(json);

        _pubSocket
            .SendMoreFrame("Logger")
            .SendMoreFrame(caller)
            .SendMoreFrame(@type)
            .SendFrame(json);
    }
}