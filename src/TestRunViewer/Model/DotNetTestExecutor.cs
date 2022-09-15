namespace TestRunViewer.Model;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Shell;
using ZmqPublisher.TestLogger;
using ZmqCollector.Collector;
using System.CodeDom.Compiler;

internal class DotNetTestExecutor
{
    private static readonly Lazy<string> _testAdapterPath = new(() =>
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            baseDirectory = baseDirectory.Replace('/', '\\');
            baseDirectory.TrimEnd('\\');
            return $"--test-adapter-path:{baseDirectory}";
        });

    public static async Task Execute(string cmd, int port)
    {
        var args = Environment.GetCommandLineArgs();
        var arg = Environment.CommandLine.Replace(args[0], string.Empty);

        var skip = 2;
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("---"))
            {
                skip = i + 1 + 1;
                i = args.Length + 2;
            }
        }

        var argss = args.Skip(skip).ToList();
        argss.Add(_testAdapterPath.Value);
        argss.Add($"--collect:{SampleDataCollector.DATA_COLLECTOR_FRIENDLY_NAME}");
        argss.Add($"--logger:{ZeroMqTestPublisher.FRIENDLY_NAME}");

        var argss2 = argss.ToList();
        argss.Add("--list-tests");

        try
        {
            await Execute(argss, argss2, port);
            // _ = Command.Run("dotnet", argss, options =>
            //                {
            //                    options.StartInfo(psi =>
            //                        {
            //                            psi.UseShellExecute = false;
            //                            psi.EnvironmentVariables.Add("ZmqCollectorPort", port.ToString());
            //                            psi.EnvironmentVariables.Add("ZmqLoggerPort", port.ToString());
            //                            psi.CreateNoWindow = true;
            //                            psi.WindowStyle = ProcessWindowStyle.Hidden;
            //                        });
            //                    options.EnvironmentVariable("ZmqCollectorPort", port.ToString());
            //                    options.EnvironmentVariable("ZmqLoggerPort", port.ToString());
            //                })
            //            .RedirectStandardErrorTo(new FileInfo("coentje1234.txt"))
            //            .RedirectTo(new FileInfo("coentje123.txt"));
            // var result = command.Task.GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task Execute(IEnumerable<object> argss, IEnumerable<object> argss2, int port)
    {
        // var cmd = Command.Run("dotnet", argss, options =>
        //                      {
        //                          options.StartInfo(psi =>
        //                              {
        //                                  psi.UseShellExecute = false;
        //                                  psi.EnvironmentVariables.Add("ZmqCollectorPort", port.ToString());
        //                                  psi.EnvironmentVariables.Add("ZmqLoggerPort", port.ToString());
        //                                  psi.CreateNoWindow = true;
        //                                  psi.WindowStyle = ProcessWindowStyle.Hidden;
        //                              });
        //                          options.EnvironmentVariable("ZmqCollectorPort", port.ToString());
        //                          options.EnvironmentVariable("ZmqLoggerPort", port.ToString());
        //                      })
        //                  .RedirectStandardErrorTo(new FileInfo("coentje1234.txt"))
        //                  .RedirectTo(new FileInfo("coentje123.txt"));
        //
        // await cmd.Task;

        var cmd2 = Command.Run("dotnet", argss2, options =>
                              {
                                  options.StartInfo(psi =>
                                      {
                                          psi.UseShellExecute = false;
                                          psi.EnvironmentVariables.Add("ZmqCollectorPort", port.ToString());
                                          psi.EnvironmentVariables.Add("ZmqLoggerPort", port.ToString());
                                          psi.CreateNoWindow = true;
                                          psi.WindowStyle = ProcessWindowStyle.Hidden;
                                      });
                                  options.EnvironmentVariable("ZmqCollectorPort", port.ToString());
                                  options.EnvironmentVariable("ZmqLoggerPort", port.ToString());
                              })
                          .RedirectStandardErrorTo(new FileInfo("coentje1234.txt"))
                          .RedirectTo(new FileInfo("coentje123.txt"));

        await cmd2.Task;
    }
}