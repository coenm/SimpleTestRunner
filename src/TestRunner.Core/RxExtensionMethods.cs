namespace TestRunner.Core;

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

public static class RxExtensionMethods
{
    // https://stackoverflow.com/questions/37412129/how-to-subscribe-with-async-method-in-rx/37412422

    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Task> asyncAction, Action<Exception>? handler = null)
    {
        async Task<Unit> Wrapped(T t)
        {
            await asyncAction();
            return Unit.Default;
        }

        return handler == null
            ? source.SelectMany(Wrapped).Subscribe(_ => { })
            : source.SelectMany(Wrapped).Subscribe(_ => { }, handler);
    }

    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<T, Task> asyncAction, Action<Exception>? handler = null)
    {
        async Task<Unit> Wrapped(T t)
        {
            await asyncAction(t);
            return Unit.Default;
        }

        return handler == null
            ? source.SelectMany(Wrapped).Subscribe(_ => { })
            : source.SelectMany(Wrapped).Subscribe(_ => { }, handler);
    }
}