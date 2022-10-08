namespace TestRunner.Core.Model;

using System;
using Interface.Data;
using TestRunViewer.Model;

public class SingleTestModel2
{
    public Guid TestCaseId { get; }

    public string DisplayName { get; }

    public SingleTestModel2(Guid testCaseId, string displayName)
    {
        TestCaseId = testCaseId;
        DisplayName = displayName;
        State = TestState.Started;
            // : TestState.Empty;
    }

    public TestState State { get; private set; }

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