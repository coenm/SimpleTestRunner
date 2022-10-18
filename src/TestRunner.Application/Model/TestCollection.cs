namespace TestRunner.Application.Model;

using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Interface.Data.Collector;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;
using Interface.Data.Logger.Inner;
using Interface.Server;
using TestRunner.Core.Model;

public class TestCollection : IDisposable/* : ICollection<SingleTestModel>*/
{
    private readonly IDisposable _subscription;

    private readonly ConcurrentDictionary<Guid, TestModel> _tests = new();

    public event EventHandler<TestModel> TestAdded = delegate { };

    public TestCollection(ITestMonitor monitor)
    {
        _ = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _subscription = monitor.Events
                               .ObserveOn(Scheduler.Default)
                               .Subscribe(data =>
                                   {
                                       // add
                                       if (data is TestRunStartEventArgsDto testRunStartEventArgsDto)
                                       {
                                           foreach (TestCaseDto testCase in testRunStartEventArgsDto.TestRunCriteria.Tests)
                                           {
                                               TryAdd(new TestModel(testCase.Id, testCase.DisplayName));
                                           }
                                       }

                                       if (data is TestCaseStartEventArgsDto testCaseStartEventArgsDto)
                                       {
                                           TestCaseDto testCase = testCaseStartEventArgsDto.TestElement;
                                           TryAdd(new TestModel(testCase.Id, testCase.DisplayName));
                                       }

                                       // update
                                       if (data is TestResultEventArgsDto testResultEventArgsDto)
                                       {
                                           if (_tests.TryGetValue(testResultEventArgsDto.Result.TestCase.Id, out TestModel? model))
                                           {
                                               model.UpdateResult(testResultEventArgsDto.Result.Outcome);
                                           }
                                       }

                                       if (data is TestCaseEndEventArgsDto testCaseEndEventArgsDto)
                                       {
                                           if (_tests.TryGetValue(testCaseEndEventArgsDto.TestCaseId, out TestModel? model))
                                           {
                                               model.UpdateResult(testCaseEndEventArgsDto.TestOutcome);
                                           }
                                       }
                                   });
    }

    private void TryAdd(TestModel model)
    {
        if (_tests.TryAdd(model.TestCaseId, model))
        {
            TestAdded?.Invoke(this, model);
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}