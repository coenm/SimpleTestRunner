namespace TestRunViewer.Model;

using System;

public class OutputEventArgs : EventArgs
{
    private readonly string _message;

    public OutputEventArgs(string message)
    {
        _message = message;
        IsCancellationRequested = false;
    }

    public string Message
    {
        get { return _message; }
    }

    public bool IsCancellationRequested { get; set; }
}