namespace TestRunViewer.ViewModel;

using System.Threading;
using System.Windows.Input;
using System;
using System.Collections.ObjectModel;
using TestRunViewer.Model;
using TestRunViewer.ViewModel.Common;

public class OutputViewModel : ViewModelBase, IOutput
{
    // private readonly RelayCommand _clearCommand;

    public OutputViewModel(SynchronizationContext synchronizationContext)
    {
        // Errors = new /*Concurrent*/ObservableCollection<ErrorViewModel>(synchronizationContext);

        // _clearCommand = new RelayCommand(x => OnClear());
    }

    #region Properties

    // public ConcurrentObservableCollection<ErrorViewModel> Errors { get; }

    #endregion // Properties

    #region IOutput implementation

    public event EventHandler<OutputEventArgs> Info = delegate { };
    public event EventHandler<OutputEventArgs> Success = delegate { };
    public event EventHandler<OutputEventArgs> Warning = delegate { };
    public event EventHandler<OutputEventArgs> Error = delegate { };
    public event EventHandler Clear = delegate { };

    #endregion // IOutput implementation

    

    // public ICommand ClearCommand
    // {
    //     get { return _clearCommand; }
    // }



    public void OnInfo(string message)
    {
        Info.Invoke(this, new OutputEventArgs(message));
    }

    public void OnInfo(string message, params object[] arguments)
    {
        Info.Invoke(this, new OutputEventArgs(string.Format(message, arguments)));
    }

    public void OnSuccess(string message)
    {
        Success.Invoke(this, new OutputEventArgs(message));
    }

    public void OnSuccess(string message, params object[] arguments)
    {
        Success.Invoke(this, new OutputEventArgs(string.Format(message, arguments)));
    }

    public void OnWarning(string message)
    {
        Warning.Invoke(this, new OutputEventArgs(message));
    }

    public void OnWarning(string message, params object[] arguments)
    {
        Warning.Invoke(this, new OutputEventArgs(string.Format(message, arguments)));
    }

    public void OnError(string message)
    {
        Error.Invoke(this, new OutputEventArgs(message));
    }

    public void OnError(string message, params object[] arguments)
    {
        Error.Invoke(this, new OutputEventArgs(string.Format(message, arguments)));
    }

    // public void OnError(string sender, IError error)
    // {
    //     OnError(error.Message);
    //
    //     BeginInvoke(() =>
    //     {
    //         var errorView = new ErrorViewModel(sender, error);
    //         Errors.Add(errorView);
    //     });
    // }

    public void OnClear()
    {
        Clear?.Invoke(this, EventArgs.Empty);
        // Errors.Clear();
    }

    public event EventHandler<OutputEventArgs> Output;
}
