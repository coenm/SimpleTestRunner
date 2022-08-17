namespace TestRunViewer.Model;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

public class AsyncActionQueue
{
    private readonly BlockingCollection<Action> _queue;
    private System.Threading.Tasks.Task _task;
    private bool _isDisposed;

    public AsyncActionQueue()
    {
        _queue = new BlockingCollection<Action>();
        _task = System.Threading.Tasks.Task.Factory.StartNew(ProcessQueue);
    }

    public void Enqueue(Action action)
    {
        _queue.Add(action);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _queue.CompleteAdding();

        if (_task != null)
        {
            _task.Wait();
            try
            {
                _task.Dispose();
            }
            catch
            {
                // Ignore
            }
            _task = null;
        }

        _queue.Dispose();
    }

    [DebuggerStepThrough]
    private void ProcessQueue()
    {
        try
        {
            while (!_queue.IsAddingCompleted)
            {
                var action = _queue.Take();

                try
                {
                    action();
                }
                catch
                {
                    // Ignore
                }
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}