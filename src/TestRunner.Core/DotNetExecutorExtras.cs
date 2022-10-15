namespace TestRunner.Core;

using System;
using System.Collections.Generic;
using Interface.Naming;

public static class DotNetExecutorExtras
{
    private static readonly Lazy<string> _testAdapterPath = new(() =>
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            baseDirectory = baseDirectory.Replace('/', '\\');
            baseDirectory.TrimEnd('\\');
            return $"{baseDirectory}";
        });

    public static IEnumerable<string> AdditionalArguments
    {
        get
        {
            yield return $"--test-adapter-path:{_testAdapterPath.Value}";
            yield return $"--collect:{PipePublisherDataCollectorNaming.DATA_COLLECTOR_FRIENDLY_NAME}";
            yield return $"--logger:{PipePublisherLoggerNaming.FRIENDLY_NAME}";
        }
    }
}