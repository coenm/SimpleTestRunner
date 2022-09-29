namespace TestRunner.Application.ViewModel;

using System;
using TestRunner.Application.Misc;
using TestRunner.Application.ViewModel.Common;

public class MainViewModel : ViewModelBase, IInitializable, IDisposable
{
    public MainViewModel(LogViewModel logViewModel)
    {
        Logs = logViewModel ?? throw new ArgumentNullException(nameof(logViewModel));
        
    }

    public void Initialize()
    {
        Logs.Initialize();
    }

    public void Dispose()
    {
    }

    public LogViewModel Logs { get; }
}