using System;

namespace Interface.Data.Logger.Base;

public abstract class BaseTestRunCriteriaBaseDto
{
    public bool KeepAlive { get; set; }

    public string TestRunSettings { get; set; }

    // public ITestHostLauncher TestHostLauncher { get; set; }

    public long FrequencyOfRunStatsChangeEvent { get; set; }

    public TimeSpan RunStatsChangeEventTimeout { get; set; }
}