using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NuGet;

namespace NuPlug
{
    public static class PackageManagerExtensions
    {
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

        public static void RemoveDuplicates(this IPackageManager packageManager, bool forceRemove = false)
        {
            var duplicates = packageManager.LocalRepository.GetPackages()
                .GroupBy(p => p.Id)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(p => p.Version).Skip(1))
                .ToList();

            duplicates.ForEach(p => packageManager.UninstallPackage(p, forceRemove, false));
        }
    }
}