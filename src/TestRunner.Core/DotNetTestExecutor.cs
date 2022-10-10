namespace TestRunner.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Interface.Naming;
using Medallion.Shell;

public class DotNetTestExecutor
{
    private readonly DotNetExecutable _executable;

    public DotNetTestExecutor(DotNetExecutable executable)
    {
        _executable = executable ?? throw new ArgumentNullException(nameof(executable));
        PipeName = $"test{DateTime.Now.Ticks}";
    }

    public string PipeName { get; }

    public async Task<int> Execute(ICollection<string> stdOut, ICollection<string> stdErr, string executable, params string[]? args)
    {
        try
        {
            var cmdArgs = new List<string>();

            if (args != null)
            {
                cmdArgs.AddRange(args);
            }

            cmdArgs.AddRange(DotNetExecutorExtras.AdditionalArguments);

            if ("dotnet".Equals(executable))
            {
                executable = _executable.Value;
            }

            Command cmd = Command.Run(
                                     executable,
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