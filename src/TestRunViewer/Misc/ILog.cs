namespace TestRunViewer.Misc;

using System;

public interface ILog
{
    void Info(string message);

    void Debug(string message);

    void Warning(string message);

    void Error(string message);

    void Timing(string operation, double elapsed);
}

public class NullLog : ILog
{
    public void Info(string message)
    {
    }

    public void Debug(string message)
    {
    }

    public void Warning(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Timing(string operation, double elapsed)
    {
    }
}

public static class LoggerExtensions
{
    public static void Warn(this ILog log, string message)
    {
        log.Warning(message);
    }

    public static void Error(this ILog log, string message, Exception ex)
    {
        log.Error(message + $" ex: {ex.Message}" );
    }
}