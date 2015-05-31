using System.ComponentModel.Composition;
using System.Diagnostics;
using Autofac;
using Autofac.Core;

namespace SamplePlugin
{
    [Export(typeof(IModule))]
    public class MyPluginModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Trace.TraceInformation("Load: " + GetType().Name);
            base.Load(builder);
        }
    }
}
