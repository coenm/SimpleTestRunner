namespace TestRunViewer.ViewModel;

using System;

public abstract class LogMessage
{
    protected LogMessage(string message)
    {
        Message = $"{DateTime.Now:HH:mm:ss.fff} - {message}";
    }

    public string Message { get; }
}

public class TimingMessage : LogMessage
{
    public TimingMessage(string message)
        : base(message)
    {
    }
}

public class WarningMessage : LogMessage
{
    public WarningMessage(string message)
        : base(message)
    {
    }
}

public class ErrorMessage : LogMessage
{
    public ErrorMessage(string message)
        : base(message)
    {
    }
}

public class InfoMessage : LogMessage
{
    public InfoMessage(string message)
        : base(message)
    {
    }
}

public class DebugMessage : LogMessage
{
    public DebugMessage(string message)
        : base(message)
    {
    }
}