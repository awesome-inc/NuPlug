using System.Collections.Generic;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// A registry interface used to ignore existing packages at runtime. 
    /// This is basically a subset of <see cref="IPackageLookup"/> without the <see cref="IPackageRepository"/> part.
    /// </summary>
    public interface IPackageRegistry
    {
        /// <summary>
        /// Checks whether a package exists in the registry.
        /// </summary>
        /// <returns>True, if found; False, otherwise.</returns>
        bool Exists(string packageId, SemanticVersion version);

        /// <summary>
        /// Finds a package in the registry. 
        /// As this is used to "fake" the existence of certain packages, the resulting package can be of type <see cref="NullPackage"/>.
        /// </summary>
        /// <returns>A valid <see cref="IPackage"/> if found; null, otherwise.</returns>
        IPackage FindPackage(string packageId, SemanticVersion version);

        /// <summary>
        /// Finds all packages for the specified <paramref name="packageId"/>.
        /// </summary>
        IEnumerable<IPackage> FindPackagesById(string packageId);

        /// <summary>
        /// Adds a package to the registry.
        /// </summary>
        void Add(string packageId, SemanticVersion version);

        /// <summary>
        /// Adds a package to the registry.
        /// </summary>
        void Add(IPackage package);
    }
}