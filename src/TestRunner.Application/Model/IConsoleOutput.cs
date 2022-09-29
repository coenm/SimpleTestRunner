namespace TestRunner.Application.Model;

using System;

public interface IConsoleOutput
{
    event EventHandler<string> StdOut;
    event EventHandler<string> StdErr;
}