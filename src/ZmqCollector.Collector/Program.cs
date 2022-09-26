// https://github.com/Microsoft/vstest-docs/blob/main/docs/analyze.md
// https://github.com/Microsoft/vstest-docs/blob/main/docs/extensions/datacollector.md
// https://github.com/microsoft/vstest/blob/main/samples/Microsoft.TestPlatform.Protocol/Program.cs
// https://github.com/Microsoft/vstest-docs/blob/main/RFCs/0007-Editors-API-Specification.md
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ZmqCollector.Collector;

using System;
using System.Collections.Generic;
using System.Xml;
using Interface.Naming;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using NetMq.Publisher;

[DataCollectorFriendlyName(SampleDataCollectorNaming.DATA_COLLECTOR_FRIENDLY_NAME)]
[DataCollectorTypeUri(SampleDataCollectorNaming.DATA_COLLECTOR_TYPE_URI)]
public class SampleDataCollector : DataCollector, ITestExecutionEnvironmentSpecifier
{
    private readonly Publisher _publisher;
    private DataCollectionEvents _events;

    public SampleDataCollector()
    {
        _publisher = new Publisher();
    }

    public override void Initialize(
        XmlElement configurationElement,
        DataCollectionEvents events,
        DataCollectionSink dataSink,
        DataCollectionLogger logger,
        DataCollectionEnvironmentContext environmentContext)
    {
        //var port = GetPort(configurationElement);
        //_publisher.Start(port);

        _events = events;
        _events.TestHostLaunched += TestHostLaunched_Handler;
        _events.SessionStart += SessionStarted_Handler;
        _events.SessionEnd += SessionEnded_Handler;
        _events.TestCaseStart += Events_TestCaseStart;
        _events.TestCaseEnd += Events_TestCaseEnd;

        // contains interesting properties?!
        //environmentContext.SessionDataCollectionContext.xxx
    }

    private static int GetPort(XmlElement configurationElement)
    {
        try
        {
            XmlElement? port = configurationElement["ZmqPort"];
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

        throw new Exception("Could not find port to connect to.");
    }
    
    private void Events_TestCaseEnd(object? sender, TestCaseEndEventArgs e)
    {
        _publisher.Send(e, "TestCaseEnd");
    }

    private void Events_TestCaseStart(object? sender, TestCaseStartEventArgs e)
    {
        _publisher.Send(e, "TestCaseStart");
    }

    private void SessionStarted_Handler(object? sender, SessionStartEventArgs e)
    {
        _publisher.Send(e, "SessionStart");
    }

    private void SessionEnded_Handler(object? sender, SessionEndEventArgs e)
    {
        _publisher.Send(e, "SessionEnded");
    }

    private void TestHostLaunched_Handler(object? sender, TestHostLaunchedEventArgs e)
    {
        _publisher.Send(e, "TestHostLaunched");
    }

    public IEnumerable<KeyValuePair<string, string>> GetTestExecutionEnvironmentVariables()
    {
        return new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("key", "value"), };
    }

    protected override void Dispose(bool disposing)
    {
        _events.TestHostLaunched -= TestHostLaunched_Handler;
        _events.SessionStart -= SessionStarted_Handler;
        _events.SessionEnd -= SessionEnded_Handler;
        _events.TestCaseStart -= Events_TestCaseStart;
        _events.TestCaseEnd -= Events_TestCaseEnd;

        _publisher.Dispose();
    }
}