namespace PipePublisher.TestLogger.Inner;

using System;
using System.Collections.Generic;
using Interface.Naming;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Pipe.Publisher;
using TestResultEventArgs = Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging.TestResultEventArgs;

public class InnerLogger : ITestLoggerWithParameters, IDisposable
{
    private readonly Publisher _publisher;
    private TestLoggerEvents _events;

    public InnerLogger()
    {
       
        _publisher = new Publisher(new ConsoleAdapter(ConsoleOutput.Instance), GetPipeName());
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        throw new NotImplementedException();
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters)
    {
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

    private static string GetPipeName()
    {
        try
        {
            var value = Environment.GetEnvironmentVariable(EnvironmentVariables.PIPE_NAME);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }
        catch (Exception)
        {
            // do nothing
        }

        throw new Exception("Could not find pipe name to connect to.");
    }

    private void EventsOnTestRunMessage(object? sender, TestRunMessageEventArgs e)
    {
        _publisher.Send(e);
    }

    private void EventsOnTestResult(object? sender, TestResultEventArgs e)
    {
        _publisher.Send(e);
    }

    private void EventsOnTestRunComplete(object? sender, TestRunCompleteEventArgs e)
    {
        _publisher.Send(e);
    }

    private void EventsOnTestRunStart(object? sender, TestRunStartEventArgs e)
    {
        _publisher.Send(e);
    }

    private void EventsOnDiscoveryMessage(object? sender, TestRunMessageEventArgs e)
    {
        _publisher.Send(e);
    }

    private void EventsOnDiscoveryComplete(object? sender, DiscoveryCompleteEventArgs e)
    {
        _publisher.Send(e);
    }

    private void EventsOnDiscoveredTests(object? sender, DiscoveredTestsEventArgs e)
    {
        _publisher.Send(e);
    }

    private void Events_DiscoveryStart(object? sender, DiscoveryStartEventArgs e)
    {
        _publisher.Send(e);
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

        _publisher.Dispose();
    }
}