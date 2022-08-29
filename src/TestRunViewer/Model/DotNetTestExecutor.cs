namespace TestRunViewer.Model;

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Medallion.Shell;
using ZmqPublisher.TestLogger;
using ZmqCollector.Collector;

internal class DotNetTestExecutor
{
    private static readonly Lazy<string> _testAdapterPath = new(() =>
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            baseDirectory = baseDirectory.Replace('/', '\\');
            baseDirectory.TrimEnd('\\');
            return $"--test-adapter-path:{baseDirectory}";
        });

    public static void Execute(string cmd, int port)
    {
        const string COLLECT_LOGGER = $" --collect:{SampleDataCollector.DATA_COLLECTOR_FRIENDLY_NAME} --logger:{ZeroMqTestPublisher.FRIENDLY_NAME} ";
        var args = Environment.GetCommandLineArgs();
        var arg = Environment.CommandLine.Replace(args[0], string.Empty);
        var cmdline1 = arg + " " + _testAdapterPath.Value + COLLECT_LOGGER;
        var cmdline = "/k " + cmdline1;

        var argss = args.Skip(2).ToList();
        argss.Add(_testAdapterPath.Value);
        argss.Add(COLLECT_LOGGER);

        var psi = new ProcessStartInfo("cmd", cmdline)
            {
                // WorkingDirectory = new FileInfo(command).DirectoryName,
                // CreateNoWindow = true,
                // UseShellExecute = false,
                // WindowStyle = ProcessWindowStyle.Hidden,
                // RedirectStandardOutput = true,
                // RedirectStandardError = true,
            };

        cmdline =  arg + " " + _testAdapterPath.Value + COLLECT_LOGGER;

        // psi = new ProcessStartInfo("wt", cmdline);
        cmdline1 = cmdline1.Trim();
        if (cmdline1.StartsWith("dotnet.exe"))
        {
            cmdline1 = cmdline1.Substring("dotnet.exe".Length, cmdline1.Length - "dotnet.exe".Length);
        }
        if (cmdline1.StartsWith("dotnet"))
        {
            cmdline1 = cmdline1.Substring("dotnet".Length, cmdline1.Length - "dotnet".Length);
        }

        psi = new ProcessStartInfo("dotnet", cmdline1.Trim());

        // Required for EnvironmentVariables to be set
        psi.UseShellExecute = false;

        psi.EnvironmentVariables.Add("ZmqCollectorPort", port.ToString());
        psi.EnvironmentVariables.Add("ZmqLoggerPort", port.ToString());

        psi.CreateNoWindow = true;
        psi.WindowStyle = ProcessWindowStyle.Hidden;


        try
        {
            // var p = Command.Run(args[1], argss, o =>
            //     {
            //         // o.EnvironmentVariables(new List<KeyValuePair<string, string>>()
            //         //     {
            //         //         new KeyValuePair<string, string>("ZmqCollectorPort", port.ToString()),
            //         //         new KeyValuePair<string, string>("ZmqLoggerPort", port.ToString()),
            //         //     });
            //     });
            // p.Wait();
            // var y = p.Task;
            var proc = Process.Start(psi);
            // var y = proc;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}