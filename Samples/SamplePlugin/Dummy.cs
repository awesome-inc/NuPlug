using System;
using System.Diagnostics;
using PluginContracts;

namespace SamplePlugin
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Dummy
    {
        public Dummy(IPluginHost pluginHost)
        {
            if (pluginHost == null) throw new ArgumentNullException(nameof(pluginHost));
            Trace.TraceInformation("Activated '{0}' from host '{1}, {2}': ", GetType().Name, pluginHost.Name, pluginHost.Version);
        }
    }
}