namespace TestRunner.Core;

using System;
using System.Collections.Generic;
using Interface.Naming;

public class DotNetExecutorExtras
{
    public static readonly Lazy<string> TestAdapterPath = new(() =>
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            baseDirectory = baseDirectory.Replace('/', '\\');
            baseDirectory.TrimEnd('\\');
            return $"--test-adapter-path:{baseDirectory}";
        });

    public static IEnumerable<string> AdditionalArguments
    {
        get
        {
            yield return TestAdapterPath.Value;
            yield return $"--collect:{PipePublisherDataCollectorNaming.DATA_COLLECTOR_FRIENDLY_NAME}";
            yield return $"--logger:{PipePublisherLoggerNaming.FRIENDLY_NAME}";
        }
    }
}