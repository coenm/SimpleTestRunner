namespace TestRunViewer.ViewModel.Common;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

public class Command : IAsyncCommand
{
    private readonly Func<object, Task> _executeAction;
    private readonly Predicate<object> _canExecuteFunc;

    public Command(Func<object, Task> executeAction, Predicate<object> canExecuteFunc = null)
    {
        _executeAction = executeAction;
        _canExecuteFunc = canExecuteFunc;
    }

    public event EventHandler CanExecuteChanged = delegate { };

    [DebuggerStepThrough]
    public async void Execute(object parameter)
    {
        await ExecuteAsync(parameter);
    }

    [DebuggerStepThrough]
    public async Task ExecuteAsync(object parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        try
        {
            await _executeAction(parameter);
        }
        catch (AggregateException ex)
        {
            MessageBox.Show($"{ex.Message}\n\n{string.Join("\n\n", ex.InnerExceptions.Select(e => e.Message))}", "Error");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error");
        }
    }

    [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return _canExecuteFunc == null || _canExecuteFunc(parameter);
    }

    public void UpdateCanExecute(object parameter = null)
    {
        CanExecuteChanged(this, EventArgs.Empty);
    }

    public void UpdateCanExecute(Action action, object parameter = null)
    {
        var beforeAction = CanExecute(parameter);
        action();
        var afterAction = CanExecute(parameter);

        if (beforeAction != afterAction)
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}