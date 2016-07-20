using System;
using System.IO;
using System.Linq;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// An <see cref="IPackageRepositoryFactory"/> basically wrapping NuGet's default factory.
    /// </summary>
    public class PackageRepositories : PackageRepositoryFactory
    {
        /// <summary>
        /// The default factory instance.
        /// </summary>
        public static IPackageRepositoryFactory Factory { get; set; } = new PackageRepositories();

        /// <summary>
        /// Creates an <see cref="IPackageRepositoryFactory"/> for the specified sources.
        /// If multiple specified, returns an <see cref="AggregateRepository"/>.
        /// </summary>
        /// <param name="packageSources">The package sources</param>
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
