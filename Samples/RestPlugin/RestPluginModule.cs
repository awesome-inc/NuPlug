using System.ComponentModel.Composition;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Integration.WebApi;
using Module = Autofac.Module;

namespace RestPlugin
{
    [Export(typeof(IModule))]
    public class RestPluginModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // cf.: http://autofac.readthedocs.org/en/latest/integration/webapi.html#register-controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterType<OwinStarter>().AutoActivate();
        }
    }
}
