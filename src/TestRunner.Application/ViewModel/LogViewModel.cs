namespace TestRunner.Application.ViewModel;

using System;
using System.IO;
using System.Threading.Tasks;
using Interface.Data.Logger;
using TestRunner.Application.Model;
using TestRunner.Application.ViewModel.Common;
using TestRunner.Core;

public class LogViewModel : ViewModelBase, IConsoleOutput
{
    private readonly ConsoleOutputProcessor _processor;
    private readonly DotNetTestExecutor _executor;
    public event EventHandler<string> StdOut = delegate { };

    public LogViewModel(ConsoleOutputProcessor processor, DotNetTestExecutor executor)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    public IDisposable Initialize()
    {
        _processor.OnEvent += ProcessorOnOnEvent;
        _processor.OnLine += ProcessorOnOnLine;
        Task.Run(async () =>
            {
                // "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject",
                StdOut.Invoke(this, "Starting..");
                var result = await _executor.Execute(
                    _processor.Out,
                    _processor.Err,
                    // "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject",
                    // "C:\\Projects\\Bdo\\git\\DRC\\Datarotonde Core",
                    "C:\\Projects\\Bdo\\git\\DRC\\Datarotonde Core Client",
                    "--filter",
                    //"Category!=single",
                    "TestCategory!=SystemTests"
                    // "FullyQualifiedName!=TestAutomation&TestCategory!=SystemTests"
                    );

                StdOut.Invoke(this, $"Ending ..");
                var dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                await File.WriteAllLinesAsync($"C:\\tmp\\coen2_{dateTime}_out.txt", _processor.Out);
                await File.WriteAllLinesAsync($"C:\\tmp\\coen2_{dateTime}_err.txt", _processor.Err);
                StdOut.Invoke(this, $"Ended {result}"); await File.WriteAllLinesAsync($"C:\\tmp\\coen2_{dateTime}_err.txt", _processor.Err);
            });
        return new Unregister();
    }

    private void ProcessorOnOnLine(object sender, string e)
    {
        StdOut.Invoke(this, e);
    }

    private void ProcessorOnOnEvent(object sender, EventArgsBaseDto e)
    {
        // StdOut.Invoke(this, $"++++ evt {e.GetType()}");
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