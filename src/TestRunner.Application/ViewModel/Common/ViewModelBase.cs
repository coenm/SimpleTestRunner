namespace TestRunner.Application.ViewModel.Common;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private readonly SynchronizationContext _context;

    protected ViewModelBase()
    {
        _context = SynchronizationContextFactory.Instance.Create();
    }

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void Post(Action action)
    {
        SynchronizationContext currentContext = SynchronizationContext.Current;
        if (currentContext != _context)
        {
            _context.Post(_ => action(), null);
        }
        else
        {
            action();
        }
    }

    protected void Send(Action action)
    {
        SynchronizationContext currentContext = SynchronizationContext.Current;
        if (currentContext != _context)
        {
            _context.Send(_ => action(), null);
        }
        else
        {
            action();
        }
    }
}