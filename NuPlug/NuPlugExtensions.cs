using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// NuPlug-specific extension methods.
    /// </summary>
    public static class NuPlugExtensions
    {
        /// <summary>
        /// Helper method to install the packages from the specified <paramref name="xml"/>.
        /// </summary>
        /// <param name="packageManager">The package manager</param>
        /// <param name="xml">The xml document specifying packages to install</param>
        /// <param name="ignoreDependencies">If false, also recursively installs all dependencies</param>
        /// <param name="allowPrerelease">If true, allows prerelase versions</param>
        /// <exception cref="AggregateException"></exception>
        public static void InstallPackages(this IPackageManager packageManager, XDocument xml,
            bool ignoreDependencies = true, bool allowPrerelease = false)
        {
            var packageIds = xml.Element("packages").Elements("package")
                .Select(x => new PackageName(x.Attribute("id").Value, 
                    SemanticVersion.Parse(x.Attribute("version").Value)))
                .ToList();

            var exceptions = new List<Exception>();

            packageIds.ForEach(p =>
            {
                try { packageManager.InstallPackage(p.Id, p.Version, ignoreDependencies, allowPrerelease); }
                catch (Exception ex) { exceptions.Add(ex); }
            });

            if (exceptions.Any())
                throw new AggregateException("Error while installing packages", exceptions);
        }

        /// <summary>
        /// Removes duplicate packages by uninstalling older packages. This is needed to resolve ambiguities for the underlying <see cref="IResolveAssembly"/>.
        /// </summary>
        /// <param name="packageManager">The package manager</param>
        /// <param name="forceRemove">If true, force uninstallation</param>
        public static void RemoveDuplicates(this IPackageManager packageManager, bool forceRemove = false)
        {
            var duplicates = packageManager.LocalRepository.GetPackages()
                .GroupBy(p => p.Id)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(p => p.Version).Skip(1))
                .ToList();

            duplicates.ForEach(p => packageManager.UninstallPackage(p, forceRemove, false));
        }

        /// <summary>
        /// Skips all packages specified in an embedded resource <paramref name="resourceName"/> of the specified <paramref name="assembly"/>.
        /// This tries finding and reading embedded resource as an <see cref="XDocument"/> with the existing packages to skip installing.
        /// </summary>
        /// <param name="skipPackages">The instance</param>
        /// <param name="assembly">The assembly</param>
        /// <param name="resourceName">The resource name</param>
        public static void SkipPackages(this ISkipPackages skipPackages, Assembly assembly = null, string resourceName = "packages.config")
        {
            var safeAssembly = assembly ?? Assembly.GetCallingAssembly();
            using (var stream = GetResourceStream(safeAssembly, resourceName))
            {
                if (stream == null) return;
                try
                {
                    var xDoc = XDocument.Load(stream);
                    SkipPackages(skipPackages, xDoc);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"Could not read embedded resource 'packages.config': {ex}");
                }
            }
        }

        /// <summary>
        /// Skips all packages declared in specified <paramref name="packagesConfig"/>.
        /// </summary>
        /// <param name="skipPackages">The instance</param>
        /// <param name="packagesConfig">The document</param>
        public static void SkipPackages(this ISkipPackages skipPackages, XDocument packagesConfig)
        {
            var xPackages = packagesConfig.Element("packages")?.Elements("package").ToList();
            var packages =
                xPackages?.Select(
                    p => new PackageName(p.Attribute("id").Value, SemanticVersion.Parse(p.Attribute("version").Value)));
            skipPackages.SkipPackages(packages);
        }

        private static Stream GetResourceStream(Assembly assembly, string resourceName)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(resourceName)) throw new ArgumentException("Resource name must not be null, empty or whitespace", nameof(resourceName));

            var name = assembly.GetManifestResourceNames()
                .OrderBy(s => s.Length)
                .FirstOrDefault(n => n.EndsWith(resourceName));
            if (string.IsNullOrWhiteSpace(name))
            {
                Trace.WriteLine($"NuPlug: Could not find embedded resource '{resourceName}'");
                return null;
            }
            return assembly.GetManifestResourceStream(name);
        }
    }
}