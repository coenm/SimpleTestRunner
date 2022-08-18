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
using TestRunViewer.Misc;
using TestRunViewer.Misc.TestMonitor;
using TestRunViewer.Model;
using TestRunViewer.ViewModel.Common;

public class MainViewModel : ViewModelBase, IInitializable, IDisposable
{
    private Task _runningTask;
    private CancellationTokenSource _runningCancellationTokenSource;
    private readonly TestMonitor _testMonitor;
    private Task _monitoringTask;
    // private readonly TestCaseFactory _testFactory;
    private object _lock =new object();

    private void TestsAdd(Guid id, string sessionId, string name, EventArgsBaseDto evt)
    {
        if (Tests.Any(x => x.Id == id && x.SessionId == sessionId))
        {
            return;
        }

        lock (_lock)
        {
            if (Tests.Any(x => x.Id == id && x.SessionId == sessionId))
            {
                return;
            }

            Tests.Add(new TestViewModel(new IdleClient(sessionId, id, name, _testMonitor, evt)));
        }
    }

    public MainViewModel()
    {
        Port = FreePortLocator.GetAvailablePort();

        Tests = new ObservableCollection<TestViewModel>();
        _testMonitor = new TestMonitor();

        // _testFactory = new TestCaseFactory(_testMonitor);

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
                                        TestsAdd(testCase.Id, data.SessionId, testCase.DisplayName, discoveredTestsEventArgsDto);
                                    }
                                }

                                if (data is TestRunStartEventArgsDto testRunStartEventArgsDto)
                                {
                                    foreach (TestCaseDto testCase in testRunStartEventArgsDto.TestRunCriteria.Tests)
                                    {
                                        TestsAdd(testCase.Id, data.SessionId, testCase.DisplayName, testRunStartEventArgsDto);
                                    }
                                }

                                if (data is TestCaseStartEventArgsDto testCaseStartEventArgsDto)
                                {
                                    TestCaseDto testCase = testCaseStartEventArgsDto.TestElement;
                                    TestsAdd(testCase.Id, data.SessionId, testCase.DisplayName, testCaseStartEventArgsDto);
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

        OnStart(null);
    }

    public int Port { get; set; }

    public ObservableCollection<TestViewModel> Tests { get; }

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
        foreach (var client in Tests)
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
        _monitoringTask = _testMonitor.StartMonitoring(Port);
        DotNetTestExecutor.Execute("", Port);
        await Task.Yield();
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

public class TestCaseFactory
{
    private readonly ITestMonitor _testMonitor;
    private readonly IDisposable _disposable;
    private int _counter = 0;

    public TestCaseFactory(ITestMonitor testMonitor)
    {
        _testMonitor = testMonitor;
        _disposable = _testMonitor.Events
                                  .Subscribe(
                                      data =>
                                          {
                                              _counter++;
                                          });
    }
}