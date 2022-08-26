namespace TestRunViewer.ViewModel.Common;

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

public class SynchronizationContextFactory
{
    private static readonly object _singletonLock = new object();
    private static SynchronizationContextFactory _instance;

    public static SynchronizationContextFactory Instance
    {
        get
        {
            SynchronizationContextFactory tmpInstance = _instance;
            if (tmpInstance != null)
            {
                return tmpInstance;
            }

            lock (_singletonLock)
            {
                return _instance ??= new SynchronizationContextFactory();
            }
        }

        set
        {
            lock (_singletonLock)
            {
                _instance = value;
            }
        }
    }

    public SynchronizationContext Create()
    {
        Dispatcher dispatcher = Application.Current != null
            ? Application.Current.Dispatcher
            : Dispatcher.CurrentDispatcher;
        return new DispatcherSynchronizationContext(dispatcher);
    }
}