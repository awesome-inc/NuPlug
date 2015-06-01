using System;
using System.Web.Http;
using PluginContracts;

namespace RestPlugin
{
    public class VersionController : ApiController
    {
        private readonly IPluginHost _pluginHost;

        public VersionController(IPluginHost pluginHost)
        {
            if (pluginHost == null) throw new ArgumentNullException(nameof(pluginHost));
            _pluginHost = pluginHost;
        }

        public string Get()
        {
            return $"{_pluginHost.Name}, {_pluginHost.Version}";
        }
    }
}