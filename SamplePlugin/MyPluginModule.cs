using System.ComponentModel.Composition;
using System.Diagnostics;
using Autofac;

namespace SamplePlugin
{
    [Export(typeof(Module))]
    public class MyPluginModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Trace.TraceInformation("Load: " + this.GetType().Name);
            base.Load(builder);
        }
    }
}
