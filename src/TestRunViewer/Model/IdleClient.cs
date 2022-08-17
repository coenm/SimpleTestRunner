namespace TestRunViewer.Model;

using System;
using System.Reactive.Linq;
using System.Threading;
using Interface.Data;
using Interface.Data.Collector;
using Interface.Data.Logger;
using Interface.Data.Logger.Inner;
using TestRunViewer.Misc;
using TestRunViewer.Misc.TestMonitor;

public enum TestState
{
    Empty,

    Started, // running

    Succeeded,

    Failed,

    Skipped,
}

public class IdleClient
{
    private readonly ILog _log;
    private bool _isDisposed;
    private int _itemCount;

    public IdleClient(string sessionId, Guid id, string name, ILog log, ITestMonitor testMonitor, EventArgsBaseDto evt)
    {
        SessionId = sessionId;
        Id = id;
        Name = name;
        _log = log;

        State = evt is TestCaseStartEventArgsDto
            ? TestState.Started
            : TestState.Empty;

        ITestMonitor testMonitor1 = testMonitor;

        testMonitor1.Events
                    .Where(x => x.SessionId == SessionId)
                    .Where(x => x is TestResultEventArgsDto result && result.Result.TestCase.Id == Id)
                    .Subscribe(x =>
                        {
                            if (x is TestResultEventArgsDto result && result.Result.TestCase.Id == Id)
                            {
                                // State = result.Result.Outcome;
                                State = Map(result.Result.Outcome);
                            }
                        });

        testMonitor1.Events
                    .Where(x => x.SessionId == SessionId)
                    .Where(x => x is TestCaseStartEventArgsDto result && result.TestCaseId == Id)
                    .Subscribe(x =>
                        {
                            State = TestState.Started;
                        });

        testMonitor1.Events
                    .Where(x => x.SessionId == SessionId)
                    .Where(x => x is TestCaseEndEventArgsDto result && result.TestCaseId == Id)
                    .Subscribe(x =>
                        {
                            if (x is TestCaseEndEventArgsDto y)
                            {
                                State = Map(y.TestOutcome);
                            }
                        });
    }

    private static TestState Map(TestOutcome resultOutcome)
    {
        return resultOutcome switch
            {
                TestOutcome.None => TestState.Empty,
                TestOutcome.Passed => TestState.Succeeded,
                TestOutcome.Failed => TestState.Failed,
                TestOutcome.Skipped => TestState.Skipped,
                TestOutcome.NotFound => TestState.Skipped,
                _ => throw new ArgumentOutOfRangeException(nameof(resultOutcome), resultOutcome, null)
            };
    }

    public event EventHandler Update = delegate { };

    public event EventHandler ItemsChanged = delegate { };

    public string SessionId { get; }

    public Guid Id { get; }

    private object _stateLock = new object();
    private TestState _state;
    public TestState State
    {
        get => _state;
        private set
        {
            if (_state == TestState.Succeeded)
            {
                return;
            }

            if (_state == TestState.Failed)
            {
                return;
            }

            if (_state == TestState.Skipped)
            {
                return;
            }

            lock (_stateLock)
            {
                if (_state == TestState.Succeeded)
                {
                    return;
                }

                if (_state == TestState.Failed)
                {
                    return;
                }

                if (_state == TestState.Skipped)
                {
                    return;
                }

                _state = value;
            }

            Update?.Invoke(this, EventArgs.Empty);
        }
    }


    public int ItemCount => _itemCount;

    public string Name { get; set; }

    public void Start()
    {
        if (_isDisposed)
        {
            return;
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;


//            _actionQueue.Dispose();
    }

    // public async Task GetItemsAsync()
    // {
    //     if (_isDisposed)
    //     {
    //         return;
    //     }
    //
    //     var token = Interlocked.Increment(ref _longRunningActionToken);
    //     _log.Info($"Client {Id} is starting GetItems {token}...");
    //
    //     var stopwatch = new Stopwatch();
    //     stopwatch.Start();
    //
    //     try
    //     {
    //         var result = await _simplexItemStore.GetItems(token);
    //
    //         stopwatch.Stop();
    //         _log.Info($"Client {Id} finished GetItems {token} in {stopwatch.Elapsed}.");
    //
    //         _itemCount = result?.Count ?? 0;
    //         ItemsChanged(this, EventArgs.Empty);
    //     }
    //     catch (Exception ex)
    //     {
    //         stopwatch.Stop();
    //         _log.Error($"Client {Id} GetItems {token} failed after {stopwatch.Elapsed}: {ex.Message}");
    //         _log.Error(ex.StackTrace);
    //     }
    // }
        
    private void ItemStoreOnItemRemoved(object sender, Guid itemId)
    {
        _log.Debug($"Client {Id} received removed item {itemId}.");
        Interlocked.Decrement(ref _itemCount);
        ItemsChanged(this, EventArgs.Empty);
    }
}