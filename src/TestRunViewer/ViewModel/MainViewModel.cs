namespace TestRunViewer.ViewModel;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

public class MainViewModel : ViewModelBase, IInitializable, IDisposable, ILog
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

            Tests.Add(new TestViewModel(new IdleClient(sessionId, id, name, this, _testMonitor, evt)));
        }
    }

    public MainViewModel()
    {
        Port = FreePortLocator.GetAvailablePort();

        Tests = new ObservableCollection<TestViewModel>();
        Messages = new ObservableCollection<LogMessage>();
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

                                LogInfo(data.GetType().Name);
                            });



        StartListening = new Command(OnStart, _ => _runningTask == null);
            
        ResetClientsCommand = new Command(OnResetClients, _ => _runningTask == null && Tests.Count > 0);
        ClearLogCommand = new Command(OnClearLog);

        Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                LogError($"Unhandled exception: {e.Exception.Message}");
                e.Handled = true;
            };
        Dispatcher.CurrentDispatcher.UnhandledException += (s, e) =>
            {
                LogError($"Unhandled exception: {e.Exception.Message}");
                e.Handled = true;
            };

        OnStart(null);
    }

    public int Port { get; set; }

    public ObservableCollection<LogMessage> Messages { get; }

    public ObservableCollection<TestViewModel> Tests { get; }

    public IAsyncCommand StartListening { get; }

    public IAsyncCommand ResetClientsCommand { get; }

    public IAsyncCommand ClearLogCommand { get; }

    public void Initialize()
    {
    }

    public void Dispose()
    {
        ResetClients();
    }

    void ILog.Info(string message)
    {
        LogInfo(message);
    }

    void ILog.Debug(string message)
    {
        LogDebug(message);
    }

    void ILog.Warning(string message)
    {
        LogWarning(message);
    }

    void ILog.Error(string message)
    {
        LogError(message);
    }

    void ILog.Timing(string operation, double elapsed)
    {
        LogTiming(operation, elapsed);
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

    // private async Task OnStartWcfDuplexClients(object arg)
    // {
    //     await Cancel().ConfigureAwait(false);
    //
    //     ResetClients();
    //
    //     await ExecuteLongRunningOperation(
    //                                       nameof(ClientViewModel.Initialize),
    //                                       cancellationToken =>
    //                                       {
    //                                           return Enumerable.Range(0, ClientCount)
    //                                                            .AsParallel()
    //                                                            .WithDegreeOfParallelism(25)
    //                                                            .WithCancellation(cancellationToken)
    //                                                            .Select(i =>
    //                                                                    {
    //                                                                        var duplexClient = new Wcf.DuplexClient(i, this);
    //                                                                        var simplexClient = new Wcf.SimplexClient();
    //                                                                        return new IdleClient(i, this, simplexClient, duplexClient, duplexClient);
    //                                                                    })
    //                                                            .Select(client =>
    //                                                                    {
    //                                                                        var clientViewModel = new ClientViewModel(client);
    //                                                                        Send(() => Clients.Add(clientViewModel));
    //                                                                        return clientViewModel;
    //                                                                    })
    //                                                            .AsThrottled()
    //                                                            .WithDegreeOfParallelism(25)
    //                                                            .WithCancellation(cancellationToken)
    //                                                            .Execute(client => Task.Run(client.Initialize, cancellationToken));
    //                                       })
    //         .ConfigureAwait(false);
    // }

    private async Task OnStart(object arg)
    {
        ResetClients();
        _monitoringTask = _testMonitor.StartMonitoring(Port);
        DotNetTestExecutor.Execute("", Port);
        await Task.Yield();

        // await Cancel().ConfigureAwait(false);

        // var r = new Random();
        // for (int i = 0; i < 10; i++)
        // {
        //     await Task.Delay(r.Next(0, 1500));
        //     Tests.Add(new TestViewModel(new IdleClient(i, this, null, null, new CommunicationChannel()))
        //         {
        //             State = State.Executing,
        //         });
        // }

        // var clientFactory = new ClientFactory(this);
        // await ExecuteLongRunningOperation(
        //                                   nameof(ClientViewModel.Initialize),
        //                                   cancellationToken =>
        //                                   {
        //                                       return Enumerable.Range(0, ClientCount)
        //                                                        .AsParallel()
        //                                                        .WithDegreeOfParallelism(25)
        //                                                        .WithCancellation(cancellationToken)
        //                                                        .Select(i =>
        //                                                                {
        //                                                                    var zeroMqClient = clientFactory.Create(i, "tcp://localhost:9656", "tcp://localhost:9657");
        //                                                                    //
        //                                                                    // var duplexClient = new Grpc.DuplexClient(i, "localhost", 9559, this);
        //                                                                    // var simplexClient = new Grpc.SimplexClient("localhost", 9556);
        //
        //                                                                    return new IdleClient(i, this, zeroMqClient, zeroMqClient, zeroMqClient);
        //                                                                })
        //                                                        .Select(client =>
        //                                                                {
        //                                                                    var clientViewModel = new ClientViewModel(client);
        //                                                                    Send(() => Clients.Add(clientViewModel));
        //                                                                    return clientViewModel;
        //                                                                })
        //                                                        .AsThrottled()
        //                                                        .WithDegreeOfParallelism(25)
        //                                                        .WithCancellation(cancellationToken)
        //                                                        .Execute(client => Task.Run(client.Initialize, cancellationToken));
        //                                   })
        //     .ConfigureAwait(false);
    }

    private Task OnResetClients(object arg)
    {
        return Task.Run(ResetClients);
    }

    /*
    private async Task OnAddItems(object arg)
    {
        await ExecuteLongRunningOperation(
                                          nameof(ClientViewModel.AddItem),
                                          async cancellationToken =>
                                          {
                                              // var clients = Clients.ToList();

                                              var clientsWithCount = Clients.ToList().Select(client => new { Client = client, client.ItemCount }).ToList();
                                              var clients = clientsWithCount.Select(x => x.Client).ToList();
                                              var clientCount = clients.Count;

                                              var stopCondition = Task.WhenAll(clientsWithCount.Select(async c =>
                                                                                                       {
                                                                                                           await c.Client.WaitForItemCountToBeGreaterOrEqual(c.ItemCount + clientCount, cancellationToken)
                                                                                                                  .ConfigureAwait(false);
                                                                                                           c.Client.ResetState();
                                                                                                       }));


                                             //
                                             // var stopCondition = Task.WhenAll(Clients.Select(
                                             //                                                  async c =>
                                             //                                                  {
                                             //                                                      await c.WaitForAddedItems(Clients.Count, cancellationToken).ConfigureAwait(false);
                                             //                                                      c.ResetState();
                                             //                                                  }));

                                              try
                                              {
                                                  foreach (var client in clients)
                                                  {
                                                      client.State = State.StartedExecution;
                                                  }

                                                  await clients.AsThrottled()
                                                               .WithCancellation(cancellationToken)
                                                               .WithDegreeOfParallelism(25)
                                                               .Execute(async client => await client.AddItem().ConfigureAwait(false))
                                                               .ConfigureAwait(false);

                                                  await stopCondition.ConfigureAwait(false);
                                              }
                                              finally
                                              {
                                                  foreach (var client in clients.Where(c => c.State == State.StartedExecution))
                                                  {
                                                      client.ResetState();
                                                  }
                                              }
                                          });
    }
    */

    /*
    private async Task OnAddItemsDuplex(object arg)
    {
        await ExecuteLongRunningOperation(
                                          nameof(ClientViewModel.AddItemDuplex),
                                          async cancellationToken =>
                                          {
                                              var clients = Clients.ToList();
                                              var stopCondition = Task.WhenAll(Clients.Select(c => c.WaitForAddedItems(Clients.Count, cancellationToken)));

                                              try
                                              {
                                                  foreach (var client in clients)
                                                  {
                                                      client.State = State.StartedExecution;
                                                  }

                                                  await clients.AsThrottled()
                                                               .WithCancellation(cancellationToken)
                                                               .WithDegreeOfParallelism(25)
                                                               .Execute(client => client.AddItemDuplex());

                                                  await stopCondition;
                                              }
                                              finally
                                              {
                                                  foreach (var client in clients.Where(c => c.State == State.StartedExecution))
                                                  {
                                                      client.ResetState();
                                                  }
                                              }
                                          });
    }
    */

        
    private async Task ExecuteLongRunningOperation(string operation, Func<CancellationToken, Task> action)
    {
        _runningCancellationTokenSource = new CancellationTokenSource();

        async Task Run()
        {
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();

                await Task.Run(() => action(_runningCancellationTokenSource.Token), _runningCancellationTokenSource.Token);

                LogTiming(operation, stopwatch.Elapsed.TotalSeconds);
            }
            catch (OperationCanceledException)
            {
                LogTiming(operation, stopwatch.Elapsed.TotalSeconds, true);
            }
            catch (Exception ex)
            {
                LogError($"Run threw exception: {ex.Message}");
            }
            finally
            {
                _runningCancellationTokenSource.Dispose();
                _runningCancellationTokenSource = null;
            }
        }

        _runningTask = Run();

        UpdateCommands();

        await _runningTask;

        _runningTask = null;

        UpdateCommands();
    }

    private Task OnClearLog(object arg)
    {
        Messages.Clear();
        return Task.FromResult(0);
    }

    private void LogInfo(string message)
    {
        var logMessage = new InfoMessage(message);
        Post(() => Messages.Insert(0, logMessage));
    }

    private void LogDebug(string message)
    {
        var logMessage = new DebugMessage(message);
        //            Post(() => Messages.Insert(0, logMessage));
    }

    private void LogWarning(string message)
    {
        var logMessage = new WarningMessage(message);
        Post(() => Messages.Insert(0, logMessage));
    }

    private void LogError(string message)
    {
        var logMessage = new ErrorMessage(message);
        Post(() => Messages.Insert(0, logMessage));
    }

    private void LogTiming(string operation, double elapsed, bool isCancelled = false)
    {
        var logMessage = new TimingMessage(isCancelled ? $"Operation {operation} is cancelled after {elapsed} seconds" : $"Operation {operation} took {elapsed} seconds to complete");
        Post(() => Messages.Insert(0, logMessage));
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