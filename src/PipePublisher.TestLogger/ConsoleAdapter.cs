namespace PipePublisher.TestLogger;

using System;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Pipe.Publisher;

internal class ConsoleAdapter : IPublisherOutput
{
    private readonly ConsoleOutput _instance;

    public ConsoleAdapter(ConsoleOutput instance)
    {
        _instance = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public void WriteLine(string? message)
    {
        _instance.WriteLine(message, OutputLevel.Information);
    }

    public void Write(string? message)
    {
        _instance.Write(message, OutputLevel.Information);
    }
}