using System.Linq;
using System.Xml.Linq;
using NuGet;
using NuGet.Packaging;

namespace NuPlug
{
    public static class PackageManagerExtensions
    {
        public static void InstallPackages(this IPackageManager packageManager, XDocument xml, 
            bool ignoreDependencies = true, bool allowPrerelease = false)
        {
            var packageIds = new PackagesConfigReader(xml)
                .GetPackages()
                .Select(p => p.PackageIdentity)
                .ToList();

            packageIds.ForEach(p => packageManager.InstallPackage(p.Id, 
                new SemanticVersion(p.Version.ToNormalizedString()), 
                ignoreDependencies, allowPrerelease));
        }
    }
}