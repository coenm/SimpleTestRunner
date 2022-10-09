namespace TestRunner.Core.Model;

using System;
using Interface.Data;
using TestRunViewer.Model;

public class SingleTestModel2
{
    private TestState _state = TestState.Empty;
    private readonly object _syncLock = new();
    public event EventHandler Update = delegate { };

    public SingleTestModel2(Guid testCaseId, string displayName)
    {
        TestCaseId = testCaseId;
        DisplayName = displayName;
        State = TestState.Started;
        // : TestState.Empty;
    }

    public Guid TestCaseId { get; }

    public string DisplayName { get; }

    public TestState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            lock (_syncLock)
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                Update?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void UpdateResult(TestOutcome resultOutcome)
    {
        State = resultOutcome switch
            {
                TestOutcome.None => TestState.Empty,
                TestOutcome.Passed => TestState.Succeeded,
                TestOutcome.Failed => TestState.Failed,
                TestOutcome.Skipped => TestState.Skipped,
                TestOutcome.NotFound => TestState.Skipped,
                _ => throw new ArgumentOutOfRangeException(nameof(resultOutcome), resultOutcome, null)
            };
    }
}