namespace TestRunViewer.Model;

using System;
using System.Reactive.Linq;
using Interface.Data;
using Interface.Data.Collector;
using Interface.Data.Logger;
using Interface.Data.Logger.Inner;
using Interface.Server;

public class SingleTestModel
{
    private bool _isDisposed;
    private readonly object _stateLock = new();
    private TestState _state;

    public SingleTestModel(string sessionId, Guid id, string name, ITestMonitor testMonitor, EventArgsBaseDto evt)
    {
        SessionId = sessionId;
        Id = id;
        Name = name;

        State = evt is TestCaseStartEventArgsDto
            ? TestState.Started
            : TestState.Empty;

        testMonitor.Events
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

        testMonitor.Events
                   .Where(x => x.SessionId == SessionId)
                   .Where(x => x is TestCaseStartEventArgsDto result && result.TestCaseId == Id)
                   .Subscribe(x =>
                       {
                           State = TestState.Started;
                       });

        testMonitor.Events
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

            if (_state == value)
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
    }
}