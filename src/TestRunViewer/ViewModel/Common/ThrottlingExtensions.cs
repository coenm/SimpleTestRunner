namespace TestRunViewer.ViewModel.Common;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class ThrottlingExtensions
{
    public static ThrottledQuery<TSource> AsThrottled<TSource>(this IEnumerable<TSource> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return new ThrottledEnumerableWrapper<TSource>(source);
    }

    public static ThrottledQuery<TSource> WithDegreeOfParallelism<TSource>(this ThrottledQuery<TSource> source, int degreeOfParallelism)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (degreeOfParallelism < 1 || degreeOfParallelism > 512)
        {
            throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));
        }

        QuerySettings settings = QuerySettings.Empty;
        settings.DegreeOfParallelism = degreeOfParallelism;

        return new ThrottledQuerySettings<TSource>(source, settings);
    }

    public static ThrottledQuery<TSource> WithCancellation<TSource>(this ThrottledQuery<TSource> source, CancellationToken cancellationToken)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var tokenRegistration = new CancellationTokenRegistration();
        try
        {
            tokenRegistration = cancellationToken.Register(() => { });
        }
        catch (ObjectDisposedException)
        {
            throw new ArgumentException(@"Can't use a disposed CancellationToken", nameof(cancellationToken));
        }
        finally
        {
            tokenRegistration.Dispose();
        }

        QuerySettings settings = QuerySettings.Empty;
        settings.CancellationToken = cancellationToken;

        return new ThrottledQuerySettings<TSource>(source, settings);
    }

    public static async Task Execute<TSource>(this ThrottledQuery<TSource> source, Func<TSource, Task> selector)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null)
        {
            throw new ArgumentNullException(nameof(selector));
        }

        var settings = source.SpecifiedQuerySettings.WithDefaults();
        using (var throttler = new SemaphoreSlim(settings.DegreeOfParallelism ?? QuerySettings.DefaultDegreeOfParallelism))
        {
            var tasks = source.Select(async s =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    await throttler.WaitAsync(settings.CancellationToken).ConfigureAwait(false);

                    settings.CancellationToken.ThrowIfCancellationRequested();

                    await Task.Factory.StartNew(() => selector(s), settings.CancellationToken, TaskCreationOptions.PreferFairness, settings.TaskScheduler)
                              .Unwrap()
                              .ConfigureAwait(false);

                    throttler.Release();
                    // ReSharper restore AccessToDisposedClosure
                });

            await Task.WhenAll(tasks);
        }
    }

    private class ThrottledEnumerableWrapper<T> : ThrottledQuery<T>
    {
        private readonly IEnumerable<T> _wrappedEnumerable;

        internal ThrottledEnumerableWrapper(IEnumerable<T> wrappedEnumerable)
            : base(QuerySettings.Empty)
        {
            _wrappedEnumerable = wrappedEnumerable;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _wrappedEnumerable.GetEnumerator();
        }
    }

    private class ThrottledQuerySettings<TSource> : ThrottledQuery<TSource>
    {
        private readonly ThrottledQuery<TSource> _source;

        internal ThrottledQuerySettings(ThrottledQuery<TSource> source, QuerySettings settings)
            : base(source.SpecifiedQuerySettings.Merge(settings))
        {
            _source = source;
        }

        public override IEnumerator<TSource> GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}

internal struct QuerySettings
{
    public TaskScheduler TaskScheduler { get; private set; }

    public int? DegreeOfParallelism { get; set; }

    public CancellationToken CancellationToken { get; set; }

    public static QuerySettings Empty => new QuerySettings(null, new int?(), new CancellationToken());

    public static int DefaultDegreeOfParallelism => Math.Min(Environment.ProcessorCount, 512);

    private QuerySettings(TaskScheduler taskScheduler, int? degreeOfParallelism, CancellationToken cancellationToken)
    {
        TaskScheduler = taskScheduler;
        DegreeOfParallelism = degreeOfParallelism;
        CancellationToken = cancellationToken;
    }

    public QuerySettings Merge(QuerySettings settings2)
    {
        if (TaskScheduler != null && settings2.TaskScheduler != null)
        {
            throw new InvalidOperationException("Can't set TaskScheduler, when it was already set.");
        }

        if (DegreeOfParallelism.HasValue && settings2.DegreeOfParallelism.HasValue)
        {
            throw new InvalidOperationException("Can't set DegreeOfParallelism, when it was already set.");
        }

        if (CancellationToken.CanBeCanceled && settings2.CancellationToken.CanBeCanceled)
        {
            throw new InvalidOperationException("Can't set CancellationToken, when it was already set.");
        }

        return new QuerySettings(TaskScheduler ?? settings2.TaskScheduler, DegreeOfParallelism ?? settings2.DegreeOfParallelism, CancellationToken.CanBeCanceled ? CancellationToken : settings2.CancellationToken);
    }

    public QuerySettings WithDefaults()
    {
        QuerySettings querySettings = this;
        if (querySettings.TaskScheduler == null)
        {
            querySettings.TaskScheduler = TaskScheduler.Default;
        }

        if (!querySettings.DegreeOfParallelism.HasValue)
        {
            querySettings.DegreeOfParallelism = DefaultDegreeOfParallelism;
        }

        return querySettings;
    }
}
    
public class ThrottledQuery<TSource> : IEnumerable<TSource>
{
    internal QuerySettings SpecifiedQuerySettings { get; }

    internal ThrottledQuery(QuerySettings settings)
    {
        SpecifiedQuerySettings = settings;
    }

    public virtual IEnumerator<TSource> GetEnumerator()
    {
        throw new NotSupportedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}