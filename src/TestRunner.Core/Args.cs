namespace TestRunner.Core;

using System;
using System.Collections.Generic;
using System.Linq;

public class Args
{
    private const string SPLITTER = "----";

    public Args(IEnumerable<string> args)
    {
        var argsArray = args.Skip(1).ToArray();
        DotNetArgs = argsArray.TakeWhile(item => !SPLITTER.Equals(item)).ToArray();

        ApplicationArgs = DotNetArgs.Length == argsArray.Length
            ? Array.Empty<string>()
            : argsArray.Skip(DotNetArgs.Length + 1).ToArray();
    }

    public string[] DotNetArgs { get; }

    public string[] ApplicationArgs { get; }
}