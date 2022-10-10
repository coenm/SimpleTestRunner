namespace TestRunner.Application.ViewModel;

using System;
using System.Linq;
using System.Threading.Tasks;
using Pipe.Server;
using TestRunner.Application.Model;
using TestRunner.Application.ViewModel.Common;
using TestRunner.Core;
using TestRunner.Core.Model;

public class LogViewModel : ViewModelBase, IConsoleOutput
{
    private readonly Args _args;
    private readonly ConsoleOutputProcessor _processor;
    private readonly DotNetTestExecutor _executor;
    private readonly PipeTestMonitor _testMonitor;
    private readonly TestCollection _testCollection;
    public event EventHandler<string> StdOut = delegate { };
    public event EventHandler<string> StdErr = delegate { };
    public event EventHandler<TestModel> TestAdded = delegate { };

    public LogViewModel(
        Args args, 
        ConsoleOutputProcessor processor,
        DotNetTestExecutor executor)
    {
        _args = args ?? throw new ArgumentNullException(nameof(args));
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _testMonitor = new PipeTestMonitor(_executor.PipeName);
        _testCollection = new TestCollection(_testMonitor);
        _testCollection.TestAdded += delegate(object sender, TestModel model)
            {
                TestAdded.Invoke(this, model);
            };
    }

    public IDisposable Initialize()
    {
        _processor.OnLine += ProcessorOnOnLine;

        if (_args.DotNetArgs.Length == 0)
        {
            return new Unregister();
        }

        Task.Run(async () =>
            {
                var args = _args.DotNetArgs;
                StdOut.Invoke(this, "Starting..");
                var result = await _executor.Execute(
                    _processor.Out,
                    _processor.Err,
                    args.First(),
                    args.Skip(1).Concat(DotNetExecutorExtras.AdditionalArguments).ToArray());
                /*
                    "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject",
                    // "C:\\Projects\\Bdo\\git\\DRC\\Datarotonde Core",
                    // "C:\\Projects\\Bdo\\git\\DRC\\Datarotonde Core Client",
                */

                StdOut.Invoke(this, $"Ended with exitcode: {result}");
            });

        return new Unregister();
    }

    private void ProcessorOnOnLine(object sender, string e)
    {
        StdOut.Invoke(this, e);
    }

    private class Unregister : IDisposable
    {
        public void Dispose()
        {
            // _processor.OnEvent -= ProcessorOnOnEvent;
            // _processor.OnLine -= ProcessorOnOnLine;
        }
    }
}