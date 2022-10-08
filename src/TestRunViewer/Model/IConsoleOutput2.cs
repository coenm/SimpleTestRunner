namespace TestRunViewer.Model;

using System;

public interface IConsoleOutput2
{
    event EventHandler<string> StdOut;
    event EventHandler<string> StdErr;
}