using System;
using System.Reflection;
using PluginContracts;

namespace ConsoleSample
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class PluginHost : IPluginHost
    {
        public PluginHost()
        {
            var assemblyName = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName();
            Name = assemblyName.Name;
            Version = assemblyName.Version.ToString();
        }

        public string Name { get; }
        public string Version { get; }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}