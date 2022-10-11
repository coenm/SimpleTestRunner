namespace Pipe.Publisher;

using System;
using Microsoft.VisualStudio.TestPlatform.Utilities;

public class ConsolePublisherOutput : IPublisherOutput
{
    public void WriteLine(string? message)
    {
        Console.WriteLine(message);
    }

    public void Write(string? message)
    {
        Console.Write(message);
    }
}