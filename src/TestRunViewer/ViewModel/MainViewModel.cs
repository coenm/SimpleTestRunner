namespace TestRunViewer.ViewModel;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Interface.Data;
using Interface.Data.Collector;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;
using Pipe.Server;
using TestRunner.Application.ViewModel;
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
    private readonly ConsoleOutputProcessor _outputProcessor;
    private readonly DotNetTestExecutor _dotNetTestExecutor;

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

    private void TestsAdd(SingleTestModel model)
    {
        if (Tests.Any(x => x.Id == model.Id))
        {
            return;
        }

        lock (_lock)
        {
            if (Tests.Any(x => x.Id == model.Id))
            {
                return;
            }

            Tests.Add(new SingleTestViewModel(model));
        }
    }

    public MainViewModel()
    {
        _outputProcessor = new ConsoleOutputProcessor();
        _dotNetTestExecutor = new DotNetTestExecutor();
        Logs = new LogViewModel(_outputProcessor, _dotNetTestExecutor);
        Tests = new ObservableCollection<SingleTestViewModel>();
        // _testMonitor = new EvtMonitor(_outputProcessor);
        // _testMonitor = new TestMonitor();
        _testMonitor = new PipeTestMonitor(_dotNetTestExecutor.PipeName);

        var uiContext = SynchronizationContext.Current;
        _testMonitor.Events
                    .Where(x => x is DiscoveredTestsEventArgsDto or TestRunStartEventArgsDto or TestCaseStartEventArgsDto)
                    .ObserveOn(Scheduler.Default)
                    //.ObserveOn(SynchronizationContext.Current!)
                    .Select(data =>
                    {
                        if (data is DiscoveredTestsEventArgsDto discoveredTestsEventArgsDto)
                        {
                            return discoveredTestsEventArgsDto.DiscoveredTestCases
                                                              .Where(dtc => Tests.All(t => t.Id != dtc.Id))
                                                              .Select(x => new SingleTestModel(x.Id, x.DisplayName, _testMonitor, discoveredTestsEventArgsDto))
                                                              .ToArray();
                            // foreach (TestCaseDto testCase in discoveredTestsEventArgsDto.DiscoveredTestCases)
                            // {
                            //     TestsAdd(testCase.Id, testCase.DisplayName, discoveredTestsEventArgsDto);
                            // }
                        }

                        if (data is TestRunStartEventArgsDto testRunStartEventArgsDto)
                        {
                            return testRunStartEventArgsDto.TestRunCriteria.Tests
                                                           .Where(dtc => Tests.All(t => t.Id != dtc.Id))
                                                           .Select(x => new SingleTestModel(x.Id, x.DisplayName, _testMonitor, testRunStartEventArgsDto))
                                                           .ToArray();
                            // foreach (TestCaseDto testCase in testRunStartEventArgsDto.TestRunCriteria.Tests)
                            // {
                            //     TestsAdd(testCase.Id, testCase.DisplayName, testRunStartEventArgsDto);
                            // }
                        }

                        if (data is TestCaseStartEventArgsDto testCaseStartEventArgsDto)
                        {
                            TestCaseDto testCase = testCaseStartEventArgsDto.TestElement;
                            if (Tests.Any(x => x.Id == testCase.Id))
                            {
                                return Array.Empty<SingleTestModel>();
                            }

                            return new[] { new SingleTestModel(testCase.Id, testCase.DisplayName, _testMonitor, testCaseStartEventArgsDto), };
                            //TestsAdd(testCase.Id, testCase.DisplayName, testCaseStartEventArgsDto);
                        }

                        //LogInfo(data.GetType().Name);
                        return Array.Empty<SingleTestModel>();
                    })
                    .ObserveOn(uiContext)
                    .Subscribe(
                        data =>
                            {
                                foreach (var item in data)
                                {
                                    TestsAdd(item);
                                }
                                // if (data is DiscoveredTestsEventArgsDto discoveredTestsEventArgsDto)
                                // {
                                //     foreach (TestCaseDto testCase in discoveredTestsEventArgsDto.DiscoveredTestCases)
                                //     {
                                //         TestsAdd(testCase.Id, testCase.DisplayName, discoveredTestsEventArgsDto);
                                //     }
                                // }
                                //
                                // if (data is TestRunStartEventArgsDto testRunStartEventArgsDto)
                                // {
                                //     foreach (TestCaseDto testCase in testRunStartEventArgsDto.TestRunCriteria.Tests)
                                //     {
                                //         TestsAdd(testCase.Id, testCase.DisplayName, testRunStartEventArgsDto);
                                //     }
                                // }
                                //
                                // if (data is TestCaseStartEventArgsDto testCaseStartEventArgsDto)
                                // {
                                //     TestCaseDto testCase = testCaseStartEventArgsDto.TestElement;
                                //     TestsAdd(testCase.Id, testCase.DisplayName, testCaseStartEventArgsDto);
                                // }

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

    public LogViewModel Logs { get; }

    public ObservableCollection<SingleTestViewModel> Tests { get; }

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
        await _dotNetTestExecutor.Execute(_outputProcessor.Out, _outputProcessor.Err, argss.First(), argss.Skip(1).ToArray());

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