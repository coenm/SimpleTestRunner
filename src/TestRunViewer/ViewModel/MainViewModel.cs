namespace TestRunViewer.ViewModel;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Interface.Data;
using Interface.Data.Collector;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;
using Interface.Server;
using NetMq.Server;
using Pipe.Server;
using TestRunner.Core;
using TestRunViewer.Misc;
using TestRunViewer.Model;
using TestRunViewer.ViewModel.Common;

public class MainViewModel : ViewModelBase, IInitializable, IDisposable
{
    private Task _runningTask;
    private CancellationTokenSource _runningCancellationTokenSource;
    private readonly PipeTestMonitor _testMonitor;
    private Task _monitoringTask;
    private object _lock = new();
    private ConsoleOutputProcessor _outputProcessor;

    private void TestsAdd(Guid id, string name, EventArgsBaseDto evt)
    {
        if (Tests.Any(x => x.Id == id))
        {
            return;
        }

        lock (_lock)
        {
            if (Tests.Any(x => x.Id == id))
            {
                return;
            }

            Tests.Add(new SingleTestViewModel(new SingleTestModel(id, name, _testMonitor, evt)));
        }
    }

    public MainViewModel()
    {
        Output = new OutputViewModel(SynchronizationContext.Current);
        Port = FreePortLocator.GetAvailablePort();
        _outputProcessor = new ConsoleOutputProcessor();
        Tests = new ObservableCollection<SingleTestViewModel>();
        // _testMonitor = new EvtMonitor(_outputProcessor);
        // _testMonitor = new TestMonitor();
        _testMonitor = new PipeTestMonitor("named_pipe_test_server");


        _testMonitor.Events
                    .Where(x => x is DiscoveredTestsEventArgsDto or TestRunStartEventArgsDto or TestCaseStartEventArgsDto)
                    .ObserveOn(SynchronizationContext.Current!)
                    .Subscribe(
                        data =>
                            {
                                if (data is DiscoveredTestsEventArgsDto discoveredTestsEventArgsDto)
                                {
                                    foreach (TestCaseDto testCase in discoveredTestsEventArgsDto.DiscoveredTestCases)
                                    {
                                        TestsAdd(testCase.Id, testCase.DisplayName, discoveredTestsEventArgsDto);
                                    }
                                }

                                if (data is TestRunStartEventArgsDto testRunStartEventArgsDto)
                                {
                                    foreach (TestCaseDto testCase in testRunStartEventArgsDto.TestRunCriteria.Tests)
                                    {
                                        TestsAdd(testCase.Id, testCase.DisplayName, testRunStartEventArgsDto);
                                    }
                                }

                                if (data is TestCaseStartEventArgsDto testCaseStartEventArgsDto)
                                {
                                    TestCaseDto testCase = testCaseStartEventArgsDto.TestElement;
                                    TestsAdd(testCase.Id, testCase.DisplayName, testCaseStartEventArgsDto);
                                }

                                //LogInfo(data.GetType().Name);
                            });



        StartListening = new Command(OnStart, _ => _runningTask == null);
            
        ResetClientsCommand = new Command(OnResetClients, _ => _runningTask == null && Tests.Count > 0);

        Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                //LogError($"Unhandled exception: {e.Exception.Message}");
                e.Handled = true;
            };
        Dispatcher.CurrentDispatcher.UnhandledException += (s, e) =>
            {
                //LogError($"Unhandled exception: {e.Exception.Message}");
                e.Handled = true;
            };

        Task.Run(async () => await OnStart(null));
    }

    public int Port { get; set; }

    public ObservableCollection<SingleTestViewModel> Tests { get; }

    public OutputViewModel Output { get; }

    public IAsyncCommand StartListening { get; }

    public IAsyncCommand ResetClientsCommand { get; }

    public void Initialize()
    {
    }

    public void Dispose()
    {
        ResetClients();
    }

    private void ResetClients()
    {
        foreach (SingleTestViewModel client in Tests)
        {
            try
            {
                client.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        Send(() => Tests.Clear());
        Post(UpdateCommands);
    }

    private async Task OnStart(object arg)
    {
        ResetClients();
        
        var args = Environment.GetCommandLineArgs();
        // var arg = Environment.CommandLine.Replace(args[0], string.Empty);

        var skip = 2;
        // for (var i = 0; i < args.Length; i++)
        // {
        //     if (args[i].Equals("---"))
        //     {
        //         skip = i + 1 + 1;
        //         i = args.Length + 2;
        //     }
        // }

        var argss = args.Skip(skip).ToList();

        argss = argss.Skip(1).ToList();

        await new DotNetTestExecutor().Execute(_outputProcessor.Out, _outputProcessor.Err, argss.First(), argss.Skip(1).ToArray());
        //_monitoringTask = _testMonitor.StartMonitoring(Port);
        // await DotNetTestExecutor.Execute("", Port).ConfigureAwait(true);
        // foreach (var x in Tests.ToList())
        // {
        //     if (x.State == State.Succeeded)
        //     {
        //         Tests.Remove(x);
        //     }
        // }
    }

    private Task OnResetClients(object arg)
    {
        return Task.Run(ResetClients);
    }

    private void UpdateCommands()
    {
        StartListening.UpdateCanExecute();

        ResetClientsCommand.UpdateCanExecute();
    }
}