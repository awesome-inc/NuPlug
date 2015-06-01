using System;
using System.Web.Http;
using PluginContracts;

namespace RestPlugin
{
    // ReSharper disable once UnusedMember.Global
    public class InfoController : ApiController
    {
        private readonly IPluginHost _pluginHost;

        public InfoController(IPluginHost pluginHost)
        {
            if (pluginHost == null) throw new ArgumentNullException(nameof(pluginHost));
            _pluginHost = pluginHost;
        }

        public void Get([FromUri]string message)
        {
            _pluginHost.WriteLine($"Message via REST: {message}");
        }
    }
}