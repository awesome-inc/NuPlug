using System;
using System.IO;
using System.Linq;
using NuGet;

namespace NuPlug
{
    public static class PackageRepositories
    {
        public static IPackageRepository Create(params string[] packageSources)
        {
            return packageSources.Count() == 1
                ? CreatePackageRepository(packageSources.Single())
                : new AggregateRepository(packageSources.Select(CreatePackageRepository));
        }
        private static IPackageRepository CreatePackageRepository(string packageSource)
        {
            var safePackageSource = packageSource;
            if (!new Uri(safePackageSource, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                safePackageSource = Path.Combine(Assemblies.HomePath, safePackageSource);

            return PackageRepositoryFactory.Default.CreateRepository(safePackageSource);
        }
    }
}