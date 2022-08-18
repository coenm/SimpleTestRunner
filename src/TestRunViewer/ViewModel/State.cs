namespace TestRunViewer.ViewModel;

public enum State
{
    NotRun,
    Skipped,
    Executing,
    Succeeded,
    Failed,

    Unknown,
    Error,
}