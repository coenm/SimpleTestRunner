namespace Pipe.Publisher;

using System;
using System.Threading;
using System.Threading.Tasks;

internal static class TaskExtensions
{
    /// <summary>
    /// See for more information: 
    /// http://stackoverflow.com/questions/14524209/what-is-the-correct-way-to-cancel-an-async-operation-that-doesnt-accept-a-cance/14524565#14524565
    /// </summary>
    public static async Task<T> WithWaitCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Register with the cancellation token.
        await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            // If the task waited on is the cancellation token...
            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        // Wait for one or the other to complete.
        return await task.ConfigureAwait(false);
    }

    public static async Task WithWaitCancellation(this Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Register with the cancellation token.
        await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            // If the task waited on is the cancellation token...
            if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        // Wait for one or the other to complete.
        await task.ConfigureAwait(false);
    }
}