using System;
using System.IO;
using System.Linq;
using NuGet;

namespace NuPlug
{
    public class PackageRepositories : PackageRepositoryFactory
    {
        public static IPackageRepositoryFactory Factory { get; set; } = new PackageRepositories();

        public static IPackageRepository For(params string[] packageSources)
        {
            return packageSources.Length == 1
                ? CreatePackageRepository(packageSources.Single())
                : new AggregateRepository(packageSources.Select(CreatePackageRepository));
        }

        private static IPackageRepository CreatePackageRepository(string packageSource)
        {
            var safePackageSource = packageSource;
            if (!new Uri(safePackageSource, UriKind.RelativeOrAbsolute).IsAbsoluteUri)
                safePackageSource = Path.Combine(Assemblies.HomePath, safePackageSource);

            return Factory.CreateRepository(safePackageSource);
        }
    }
}
