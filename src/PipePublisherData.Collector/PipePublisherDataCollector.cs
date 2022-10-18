// https://github.com/Microsoft/vstest-docs/blob/main/docs/analyze.md
// https://github.com/Microsoft/vstest-docs/blob/main/docs/extensions/datacollector.md
// https://github.com/microsoft/vstest/blob/main/samples/Microsoft.TestPlatform.Protocol/Program.cs
// https://github.com/Microsoft/vstest-docs/blob/main/RFCs/0007-Editors-API-Specification.md
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PipePublisherData.Collector;

using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

[DataCollectorFriendlyName("pipe-publisher-collector")] //PipePublisherDataCollectorNaming.DATA_COLLECTOR_FRIENDLY_NAME)]
[DataCollectorTypeUri("https://github.com/coenm/pipepublishercollector")] //PipePublisherDataCollectorNaming.DATA_COLLECTOR_TYPE_URI)]
public class PipePublisherDataCollector : DataCollector
{
    private readonly PluginLoadContext _loadContext;
    private readonly Assembly _assembly;
    private readonly DataCollector? _collector;

    public PipePublisherDataCollector()
    {
        var dllFileName = GetDllFileName();
        _loadContext = new PluginLoadContext(dllFileName);
        _assembly = _loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(dllFileName));
        Type? t = _assembly?.GetTypes().SingleOrDefault(type => typeof(DataCollector).IsAssignableFrom(type));
        if (t != null)
        {
            _collector = Activator.CreateInstance(t) as DataCollector;
        }
    }

    public override void Initialize(
        XmlElement? configurationElement,
        DataCollectionEvents events,
        DataCollectionSink dataSink,
        DataCollectionLogger logger,
        DataCollectionEnvironmentContext? environmentContext)
    {
        _collector?.Initialize(
            configurationElement,
            events,
            dataSink,
            logger,
            environmentContext);
    }

    private static string GetDllFileName()
    {
        try
        {
            var value = Environment.GetEnvironmentVariable("TEST_PLUGIN_COLLECTOR_DLL");
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

    protected override void Dispose(bool disposing)
    {
        (_collector as IDisposable)?.Dispose();
        _loadContext.Unload();
    }
}