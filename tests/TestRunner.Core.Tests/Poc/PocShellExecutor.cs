namespace TestRunner.Core.Tests.Poc;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Medallion.Shell;

public static class PocShellExecutor
{
    public static async Task<bool> Execute(ICollection<string> collection1, ICollection<string> collection2, string arg)
    {
        try
        {
            Command cmd = Command.Run("TestConsoleApp.exe", new object[] { arg, })
                                 .RedirectTo(collection1)
                                 .RedirectStandardErrorTo(collection2);
            CommandResult result = await cmd.Task;
            return result.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static async Task<bool> Execute(Stream stdOut, string arg)
    {
        try
        {
            Command cmd = Command.Run("TestConsoleApp.exe", new object[] { arg, })
                                 .RedirectTo(stdOut);
            CommandResult result = await cmd.Task;
            return result.Success;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}