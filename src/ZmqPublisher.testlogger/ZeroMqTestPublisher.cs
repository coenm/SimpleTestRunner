// https://github.com/Microsoft/vstest-docs/blob/main/docs/analyze.md
// https://github.com/Microsoft/vstest-docs/blob/main/docs/extensions/datacollector.md
// https://github.com/microsoft/vstest/blob/main/samples/Microsoft.TestPlatform.Protocol/Program.cs
// https://github.com/Microsoft/vstest-docs/blob/main/RFCs/0007-Editors-API-Specification.md

namespace ZmqPublisher.TestLogger;

using System;
using System.Collections.Generic;
using Interface;
using Interface.Naming;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NetMq.Publisher;
using TestResultEventArgs = Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestResultEventArgs;

[FriendlyName(TestLoggerNaming.FRIENDLY_NAME)]
[ExtensionUri(TestLoggerNaming.EXTENSION_URI)]
public class ZeroMqTestPublisher : ITestLoggerWithParameters, IDisposable
{
    private readonly IOutput _output;
    private readonly Publisher _publisher;
    private TestLoggerEvents _events;

    public ZeroMqTestPublisher()
    {
        // _output = output;
        _publisher = new Publisher(ConsoleOutput.Instance);
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        throw new NotImplementedException();
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
    {
        //var port = GetPort(parameters);
        //_publisher.Start(port);

        _events = events;
        _events.DiscoveryStart += Events_DiscoveryStart;
        _events.DiscoveredTests += EventsOnDiscoveredTests;
        _events.DiscoveryComplete += EventsOnDiscoveryComplete;
        _events.DiscoveryMessage += EventsOnDiscoveryMessage;

        _events.TestRunStart += EventsOnTestRunStart;
        _events.TestRunComplete += EventsOnTestRunComplete;
        _events.TestResult += EventsOnTestResult;
        _events.TestRunMessage += EventsOnTestRunMessage;
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

        throw new Exception("Could not find port to connect to.");
    }

    private void EventsOnTestRunMessage(object? sender, TestRunMessageEventArgs e)
    {
        _publisher.Send(e, "TestRunMessage");
    }

    private void EventsOnTestResult(object? sender, TestResultEventArgs e)
    {
        _publisher.Send(e, "TestResult");
    }

    private void EventsOnTestRunComplete(object? sender, TestRunCompleteEventArgs e)
    {
        _publisher.Send(e, "TestRunComplete");
    }

    private void EventsOnTestRunStart(object? sender, TestRunStartEventArgs e)
    {
        _publisher.Send(e, "TestRunStart");
    }

    private void EventsOnDiscoveryMessage(object? sender, TestRunMessageEventArgs e)
    {
        _publisher.Send(e, "DiscoveryMessage");
    }

    private void EventsOnDiscoveryComplete(object? sender, DiscoveryCompleteEventArgs e)
    {
        _publisher.Send(e, "DiscoveryComplete");
    }

    private void EventsOnDiscoveredTests(object? sender, DiscoveredTestsEventArgs e)
    {
        _publisher.Send(e, "DiscoveredTests");
    }

    private void Events_DiscoveryStart(object? sender, DiscoveryStartEventArgs e)
    {
        _publisher.Send(e, "DiscoveryStart");
    }

    public void Dispose()
    {
        _events.DiscoveryStart -= Events_DiscoveryStart;
        _events.DiscoveredTests -= EventsOnDiscoveredTests;
        _events.DiscoveryComplete -= EventsOnDiscoveryComplete;
        _events.DiscoveryMessage -= EventsOnDiscoveryMessage;

        _events.TestRunStart -= EventsOnTestRunStart;
        _events.TestRunComplete -= EventsOnTestRunComplete;
        _events.TestResult -= EventsOnTestResult;
        _events.TestRunMessage -= EventsOnTestRunMessage;

        ConsoleOutput.Instance.Write("--- Disposing Logger ----" + Environment.NewLine, OutputLevel.Information);
        _publisher.Dispose();
        ConsoleOutput.Instance.Write("--- Disposed Logger ----" + Environment.NewLine, OutputLevel.Information);
    }
}