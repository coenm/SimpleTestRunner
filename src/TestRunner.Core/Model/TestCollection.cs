namespace TestRunner.Core.Model;

using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Interface.Data.Collector;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;
using Interface.Data.Logger.Inner;
using Interface.Server;

public class TestCollection : IDisposable/* : ICollection<SingleTestModel>*/
{
    private readonly IDisposable _subscription;

    private readonly ConcurrentDictionary<Guid, SingleTestModel2> _tests = new();

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
                                               _tests.TryAdd(testCase.Id, new SingleTestModel2(testCase.Id, testCase.DisplayName));
                                           }
                                       }

                                       if (data is TestCaseStartEventArgsDto testCaseStartEventArgsDto)
                                       {
                                           TestCaseDto testCase = testCaseStartEventArgsDto.TestElement;
                                           _tests.TryAdd(testCase.Id, new SingleTestModel2(testCase.Id, testCase.DisplayName /*started*/));
                                       }

                                       // update
                                       if (data is TestResultEventArgsDto testResultEventArgsDto)
                                       {
                                           if (_tests.TryGetValue(testResultEventArgsDto.Result.TestCase.Id, out SingleTestModel2? model))
                                           {
                                               model.UpdateResult(testResultEventArgsDto.Result.Outcome);
                                           }
                                       }

                                       if (data is TestCaseEndEventArgsDto testCaseEndEventArgsDto)
                                       {
                                           if (_tests.TryGetValue(testCaseEndEventArgsDto.TestCaseId, out SingleTestModel2? model))
                                           {
                                               model.UpdateResult(testCaseEndEventArgsDto.TestOutcome);
                                           }
                                       }
                                   });
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}