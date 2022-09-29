namespace TestRunViewer.Model;

using System;

public interface IOutput
{
    event EventHandler<OutputEventArgs> Output;
    // event EventHandler<ErrorEventArgs> Error;
}