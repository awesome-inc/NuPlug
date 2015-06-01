using System.ComponentModel.Composition;
using Autofac;
using Autofac.Core;

namespace SamplePlugin
{
    [Export(typeof(IModule))]
    public class SamplePluginModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Dummy>().AutoActivate();
        }
    }
}
