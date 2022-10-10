namespace TestRunner.Application.ViewModel;

using System;
using System.IO;
using System.Threading.Tasks;
using Pipe.Server;
using TestRunner.Application.Model;
using TestRunner.Application.ViewModel.Common;
using TestRunner.Core;
using TestRunner.Core.Model;

public class LogViewModel : ViewModelBase, IConsoleOutput
{
    private readonly ConsoleOutputProcessor _processor;
    private readonly DotNetTestExecutor _executor;
    private readonly PipeTestMonitor _testMonitor;
    private readonly TestCollection _testCollection;
    public event EventHandler<string> StdOut = delegate { };
    public event EventHandler<string> StdErr = delegate { };
    public event EventHandler<TestModel> TestAdded = delegate { };

    public LogViewModel(ConsoleOutputProcessor processor, DotNetTestExecutor executor)
    {
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
        Task.Run(async () =>
            {
                // "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject",
                StdOut.Invoke(this, "Starting..");
                var result = await _executor.Execute(
                    _processor.Out,
                    _processor.Err,
                    "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject",
                    // "C:\\Projects\\Bdo\\git\\DRC\\Datarotonde Core",
                    // "C:\\Projects\\Bdo\\git\\DRC\\Datarotonde Core Client",
                    "--filter",
                    //"Category!=single",
                    "TestCategory!=SystemTests"
                    // "FullyQualifiedName!=TestAutomation&TestCategory!=SystemTests"
                    );

                StdOut.Invoke(this, $"Ending ..");
                var dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                // await File.WriteAllLinesAsync($"C:\\tmp\\coen2_{dateTime}_out.txt", _processor.Out);
                // await File.WriteAllLinesAsync($"C:\\tmp\\coen2_{dateTime}_err.txt", _processor.Err);
                StdOut.Invoke(this, $"Ended {result}");
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