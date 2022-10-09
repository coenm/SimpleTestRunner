namespace TestRunner.Application.ViewModel;

public enum TestState
{
    NotRun,
    Skipped,
    Executing,
    Succeeded,
    Failed,

    Unknown,
    Error,
}