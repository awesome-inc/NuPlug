using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Hosting;

namespace WebApiPlugin
{
    class OwinStarter
    {
        public OwinStarter(ILifetimeScope container)
        {
            OwinWebApiConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            WebApp.Start<OwinWebApiConfig>("http://localhost:9000/");
        }
    }
}