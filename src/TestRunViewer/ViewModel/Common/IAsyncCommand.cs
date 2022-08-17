namespace TestRunViewer.ViewModel.Common;

using System;
using System.Threading.Tasks;
using System.Windows.Input;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object parameter);

    void UpdateCanExecute(Action action, object parameter = null);

    void UpdateCanExecute(object parameter = null);
}