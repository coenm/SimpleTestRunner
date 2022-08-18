// https://github.com/Microsoft/vstest-docs/blob/main/docs/analyze.md
// https://github.com/Microsoft/vstest-docs/blob/main/docs/extensions/datacollector.md
// https://github.com/microsoft/vstest/blob/main/samples/Microsoft.TestPlatform.Protocol/Program.cs
// https://github.com/Microsoft/vstest-docs/blob/main/RFCs/0007-Editors-API-Specification.md
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Xml;
using AutoMapper;
using Interface;
using Interface.Data.Logger;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;

namespace ZmqCollector.Collector;

[DataCollectorFriendlyName("zmq-publisher-collector")]
[DataCollectorTypeUri("my://sample/datacollector")]
public class SampleDataCollector : DataCollector, ITestExecutionEnvironmentSpecifier
{
    private readonly PublisherSocket _pubSocket;
    private readonly IMapper _mapper;
    private readonly string _session = Guid.NewGuid().ToString();

    public SampleDataCollector()
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
        _mapper = config.CreateMapper();

        _pubSocket = new PublisherSocket();
        _pubSocket.Options.SendHighWatermark = 1000;
    }

    public override void Initialize(
        XmlElement configurationElement,
        DataCollectionEvents events,
        DataCollectionSink dataSink,
        DataCollectionLogger logger,
        DataCollectionEnvironmentContext environmentContext)
    {
        events.TestHostLaunched += TestHostLaunched_Handler;
        events.SessionStart += SessionStarted_Handler;
        events.SessionEnd += SessionEnded_Handler;
        events.TestCaseStart += Events_TestCaseStart;
        events.TestCaseEnd += Events_TestCaseEnd;

        // todo ubsubscribe
        int port = GetPort(configurationElement);
        _pubSocket.Connect($"tcp://localhost:{port}");
    }

    private static int GetPort(XmlElement configurationElement)
    {
        try
        {
            var port = configurationElement["ZmqPort"];
            if (port != null)
            {
                var portValue = port.InnerText;
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
            var portValue = Environment.GetEnvironmentVariable("ZmqCollectorPort");
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

    private void Send(EventArgs evt, string caller)
    {
        var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
        dto!.SessionId = _session;
        var json = JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
        var @type = dto.GetType().Name;
        Console.WriteLine($"Send {@type}");
        Console.WriteLine(json);
          
        _pubSocket
            .SendMoreFrame("DataCollector")
            .SendMoreFrame(caller)
            .SendMoreFrame(@type)
            .SendFrame(json);
    }

    private void Events_TestCaseEnd(object? sender, TestCaseEndEventArgs e)
    {
        Send(e, "TestCaseEnd");
    }

    private void Events_TestCaseStart(object? sender, TestCaseStartEventArgs e)
    {
        Send(e, "TestCaseStart");
    }

    private void SessionStarted_Handler(object? sender, SessionStartEventArgs e)
    {
        Send(e, "SessionStart");
    }

    private void SessionEnded_Handler(object? sender, SessionEndEventArgs e)
    {
        Send(e, "SessionEnded");
    }

    private void TestHostLaunched_Handler(object? sender, TestHostLaunchedEventArgs e)
    {
        Send(e, "TestHostLaunched");
    }

    public IEnumerable<KeyValuePair<string, string>> GetTestExecutionEnvironmentVariables()
    {
        return new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value") };
    }

    protected override void Dispose(bool disposing)
    {
        _pubSocket?.Dispose();
    }
}