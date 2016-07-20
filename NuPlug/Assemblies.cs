using System;
using System.IO;
using System.Reflection;

namespace NuPlug
{
    internal static class Assemblies
    {
        public static readonly string HomePath = GetDirectory(null);

        public static string GetFullPath(string packageSource)
        {
            var safePackageSource = Environment.ExpandEnvironmentVariables(packageSource);
            if (!new Uri(safePackageSource, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                safePackageSource = Path.Combine(HomePath, safePackageSource);
            return safePackageSource;
        }

        public static string GetLocation(this Assembly assembly)
        {
            var safeAssembly = assembly ?? (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
            var codeBase = safeAssembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var assemblyFile = Uri.UnescapeDataString(uri.Path);
            return Path.GetFullPath(assemblyFile);
        }

        public static string GetDirectory(this Assembly assembly)
        {
            var assemblyFile = GetLocation(assembly);
            return Path.GetDirectoryName(assemblyFile) ?? string.Empty;
        }
    }
}