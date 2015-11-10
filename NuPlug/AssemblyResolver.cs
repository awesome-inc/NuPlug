using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NuPlug
{
    internal class AssemblyResolver : IResolveAssembly
    {
        private bool _isDisposed;
        public IList<string> Directories { get; }

        public AssemblyResolver(IEnumerable<string> directories = null)
        {
            Directories = (directories ?? Enumerable.Empty<string>()).ToList();
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            AppDomain.CurrentDomain.AssemblyResolve -=  ResolveAssembly;
            _isDisposed = true;
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (!Directories.Any()) return null;

            var nameTokens = args.Name.Split(',');
            var assemblyName = nameTokens[0];

            // skip resources
            if (assemblyName.EndsWith(".resources")) return null;

            var requestedVersion = nameTokens.Length > 1 ? new Version(nameTokens[1].Split('=')[1]) : null;

            // avoid loading assembly multiple times
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => Matching(a.GetName(), assemblyName, requestedVersion));

            if (assembly == null)
            {
                // find most recent (implicit binding redirect)
                var fileName = Directories.SelectMany(d => Directory.EnumerateFiles(d, assemblyName + ".dll"))
                    .OrderByDescending(f => AssemblyName.GetAssemblyName(f).Version)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(fileName)) return null;
                assembly = Assembly.LoadFrom(fileName);
            }

            var foundFile = assembly.GetLocation();
            var foundVersion = assembly.GetName().Version;

            Trace.WriteLine(requestedVersion == null || foundVersion == requestedVersion
                ? $"Resolved '{assemblyName}, {requestedVersion}' from '{foundFile}'..."
                : $"Resolved '{assemblyName}, {requestedVersion} -> {foundVersion}' from '{foundFile}'...");

            return assembly;
        }

        private static bool Matching(AssemblyName assemblyName, string name, Version version)
        {
            return assemblyName.Name == name && (version == null || version >= assemblyName.Version);
        }
    }
}