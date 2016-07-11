using System;
using System.Reflection;
using PluginContracts;

namespace ConsoleSample
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PluginHost : IPluginHost
    {
        public PluginHost()
        {
            var assemblyName = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName();
            Name = assemblyName.Name;
#if NCRUNCH
            Version = assemblyName.Version.ToString();
#else
            Version = GitVersionInformation.InformationalVersion;
#endif
        }

        public string Name { get; }
        public string Version { get; }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}