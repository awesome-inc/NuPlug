using System.Threading.Tasks;

namespace PluginContracts
{
    public interface IPluginHost
    {
        string Name { get; }
        string Version { get; }

        void WriteLine(string message);
    }
}
