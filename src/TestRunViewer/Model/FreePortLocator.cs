namespace TestRunViewer.Model;

using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using TestRunViewer.ViewModel.Common;

internal class FreePortLocator
{
    private static readonly IPEndPoint _defaultLoopbackEndpoint = new(IPAddress.Loopback, port: 0);

    public static int GetAvailablePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(_defaultLoopbackEndpoint);
        return (((IPEndPoint)socket.LocalEndPoint)!).Port;
    }
}

internal class DotNetTestExecutor
{
    public static void Execute(string cmd, int port)
    {
        const string ADAPTER_PATH = ""; //@" --test-adapter-path:C:\Projects\coenm\TestCollector\TestProject\bin\Debug\net6.0 ";
        const string COLLECT_LOGGER = " --collect:zmq-publisher-collector  --logger:zmq-test-publisher ";
        var args = Environment.GetCommandLineArgs();
        var arg = Environment.CommandLine.Replace(args[0], string.Empty);
        var cmdline = "/k " + arg + " " + ADAPTER_PATH + COLLECT_LOGGER;

        var psi = new ProcessStartInfo("cmd", cmdline)
        {
            // WorkingDirectory = new FileInfo(command).DirectoryName,
            // CreateNoWindow = true,
            // UseShellExecute = false,
            // WindowStyle = ProcessWindowStyle.Hidden,
            // RedirectStandardOutput = true,
            // RedirectStandardError = true,
        };

        cmdline =  arg + " " + ADAPTER_PATH + COLLECT_LOGGER;
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