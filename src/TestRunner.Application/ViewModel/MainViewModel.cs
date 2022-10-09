namespace TestRunner.Application.ViewModel;

using System;
using System.Collections.ObjectModel;
using TestRunner.Application.Misc;
using TestRunner.Application.ViewModel.Common;

public class MainViewModel : ViewModelBase, IInitializable, IDisposable
{
    public MainViewModel(LogViewModel logViewModel)
    {
        Logs = logViewModel ?? throw new ArgumentNullException(nameof(logViewModel));
        Tests = new ObservableCollection<TestsViewModel>();
        Logs.TestAdded += (sender, model) =>
            {
                Post(() => Tests.Add(new TestsViewModel(model)));
            };
    }

    public void Initialize()
    {
        Logs.Initialize();
    }

    public void Dispose()
    {
    }

    public LogViewModel Logs { get; }

    public ObservableCollection<TestsViewModel> Tests { get; }
}