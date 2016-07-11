using System.Diagnostics;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;

namespace RestPlugin
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class OwinStarter
    {
        public OwinStarter(ILifetimeScope container)
        {
            const string endpoint = "http://localhost:9000/";

            OwinWebApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            WebApp.Start<OwinWebApiConfig>(endpoint);

            Trace.TraceInformation($"Started REST endpoint at '{endpoint}'.");
            Trace.TraceInformation($"Browse REST API docs at '{endpoint}swagger/'");
        }
    }
}