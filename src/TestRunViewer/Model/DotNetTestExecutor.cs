namespace TestRunViewer.Model;

using System;
using System.Diagnostics;
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
        // const string COLLECT_LOGGER = " --collect:zmq-publisher-collector  --logger:zmq-test-publisher ";
        const string COLLECT_LOGGER = $" --collect:{SampleDataCollector.DATA_COLLECTOR_FRIENDLY_NAME} --logger:{ZeroMqTestPublisher.FRIENDLY_NAME} ";
        var args = Environment.GetCommandLineArgs();
        var arg = Environment.CommandLine.Replace(args[0], string.Empty);
        var cmdline = "/k " + arg + " " + _testAdapterPath.Value + COLLECT_LOGGER;

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
        psi = new ProcessStartInfo("wt", cmdline);

        // cmdline = arg + " " + ADAPTER_PATH + COLLECT_LOGGER;
        // psi = new ProcessStartInfo("dotnet", cmdline)
        //     {
        //         CreateNoWindow = true,
        //         UseShellExecute = false,
        //     };

        psi.EnvironmentVariables.Add("ZmqCollectorPort", port.ToString());
        psi.EnvironmentVariables.Add("ZmqLoggerPort", port.ToString());
        var proc = Process.Start(psi);
    }
}