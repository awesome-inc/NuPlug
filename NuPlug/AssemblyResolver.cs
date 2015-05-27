using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NuPlug
{
    class AssemblyResolver : IDisposable
    {
        private readonly ResolveEventHandler _handler;

        public AssemblyResolver(string directory)
        {
            _handler = AssemblyResolverFor(directory);
            AppDomain.CurrentDomain.AssemblyResolve += _handler;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= _handler;
        }

        private static ResolveEventHandler AssemblyResolverFor(string libDir)
        {
            return (sender, args) =>
            {
                var fileName = args.Name.Remove(args.Name.IndexOf(',')) + ".dll";
                var foundFile = Directory.EnumerateFiles(libDir, fileName, SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(foundFile)) return null;
                var assembly = Assembly.LoadFrom(foundFile);
                Trace.WriteLine($"Resolved '{fileName}' from '{assembly.FullName}'...");
                return assembly;
            };
        }
    }
}