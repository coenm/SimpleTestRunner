namespace TestRunViewer.Model;

public class ErrorEventArgs : OutputEventArgs
{
    // private readonly IError _error;

    public ErrorEventArgs(string message)
        : base(message)
    {
        IsCancellationRequested = false;
    }
}