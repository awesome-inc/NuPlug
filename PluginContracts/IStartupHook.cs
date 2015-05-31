using Autofac;

namespace PluginContracts
{
    public interface IStartupHook
    {
        void OnStartup(IContainer container);
    }
}
