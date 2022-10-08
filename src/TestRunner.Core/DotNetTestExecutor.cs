namespace TestRunner.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Interface.Naming;
using Medallion.Shell;

public class DotNetTestExecutor
{
    public DotNetTestExecutor(string pipeName)
    {
        PipeName = pipeName;
    }

    public DotNetTestExecutor()
    : this ($"test{DateTime.Now.Ticks}")
    {
    }

    public string PipeName { get; }
    
    public async Task<int> Execute(ICollection<string> stdOut, ICollection<string> stdErr, string project, params string[]? args)
    {
        try
        {
            var cmdArgs = new List<string>
                {
                    "test",
                    project,
                };

            if (args != null)
            {
                cmdArgs.AddRange(args);
            }

            cmdArgs.AddRange(DotNetExecutorExtras.AdditionalArguments);

            Command cmd = Command.Run(
                                     "dotnet",
                                     cmdArgs,
                                     options => options.EnvironmentVariable(EnvironmentVariables.PIPE_NAME, PipeName))
                                 .RedirectTo(stdOut)
                                 .RedirectStandardErrorTo(stdErr);

            CommandResult result = await cmd.Task.ConfigureAwait(false);
            
            return result.ExitCode;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}