namespace PipePublisher.TestLogger;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Interface.Naming;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

[FriendlyName(PipePublisherLoggerNaming.FRIENDLY_NAME)]
[ExtensionUri(PipePublisherLoggerNaming.EXTENSION_URI)]
public class PipePublisherLogger : ITestLoggerWithParameters, IDisposable
{
    private readonly PluginLoadContext _loadContext;
    private readonly ITestLoggerWithParameters? _testLogger;

    public PipePublisherLogger()
    {
        var dllFileName = GetDllFileName();
        _loadContext = new PluginLoadContext(dllFileName);
        Assembly assembly = _loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(dllFileName));
        Type? t = assembly.GetTypes().SingleOrDefault(type => typeof(ITestLoggerWithParameters).IsAssignableFrom(type));
        if (t != null)
        {
            _testLogger = Activator.CreateInstance(t) as ITestLoggerWithParameters;
        }
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory)
    {
        _testLogger?.Initialize(events, testRunDirectory);
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters)
    {
        _testLogger?.Initialize(events, parameters);
    }

    private static string GetDllFileName()
    {
        try
        {
            var value = Environment.GetEnvironmentVariable(EnvironmentVariables.TEST_PLUGIN_LOGGER_DLL);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }
        catch (Exception)
        {
            // do nothing
        }

        throw new Exception("Could not find pipe name to connect to.");
    }

    public void Dispose()
    {
        (_testLogger as IDisposable)?.Dispose();
        _loadContext.Unload();
    }
}