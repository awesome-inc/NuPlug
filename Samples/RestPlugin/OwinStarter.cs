using System.Diagnostics;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;

namespace RestPlugin
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class OwinStarter
    {
        public OwinStarter(ILifetimeScope container)
        {
            var endpoint = "http://localhost:9000/";

            OwinWebApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            WebApp.Start<OwinWebApiConfig>(endpoint);

            Trace.TraceInformation("Started REST endpoint at '{0}'", endpoint);
        }
    }
}