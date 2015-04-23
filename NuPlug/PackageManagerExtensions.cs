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
                .Select(x => new PackageName(x.Attribute("id").Value, SemanticVersion.Parse(x.Attribute("version").Value)))
                .ToList();

            packageIds.ForEach(p => packageManager.InstallPackage(p.Id, p.Version, ignoreDependencies, allowPrerelease));
        }
    }
}