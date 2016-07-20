using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// A custom <see cref="IPackageRegistry"/> used to ignore existing packages at runtime. 
    /// </summary>
    public class NuPlugPackageRegistry : IPackageRegistry
    {
        private readonly IDictionary<string, IDictionary<SemanticVersion, IPackage>> _packages =
            new Dictionary<string, IDictionary<SemanticVersion, IPackage>>();

        /// <summary>
        /// Check whether a package exists in the registry
        /// </summary>
        /// <returns>True, if found; False, otherwise.</returns>
        public bool Exists(string packageId, SemanticVersion version = null)
        {
            IDictionary<SemanticVersion, IPackage> versions;
            if (_packages.TryGetValue(packageId, out versions))
            {
                return version == null || versions.ContainsKey(version);
            }

            var libFile = Path.Combine(Assemblies.HomePath, $"{packageId}.dll");
            return File.Exists(libFile);
        }

        /// <summary>
        /// Finds a package in the registry. 
        /// As this is used to "fake" the existence of certain packages, the resulting package can be of type <see cref="NullPackage"/>.
        /// </summary>
        /// <returns>A valid <see cref="IPackage"/> if found; null, otherwise.</returns>
        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            if (string.IsNullOrWhiteSpace(packageId)) throw new ArgumentNullException(nameof(packageId));
            if (version == null) throw new ArgumentNullException(nameof(version));

            IDictionary<SemanticVersion, IPackage> versions;
            if (_packages.TryGetValue(packageId, out versions))
            {
                IPackage package;
                if (versions.TryGetValue(version, out package))
                    return package;
            }
            return null;
        }

        /// <summary>
        /// Finds all packages for the specified <paramref name="packageId"/>.
        /// </summary>
        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId)) throw new ArgumentNullException(nameof(packageId));
            IDictionary<SemanticVersion, IPackage> versions;
            if (_packages.TryGetValue(packageId, out versions))
                return versions.Values.ToList();
            return Enumerable.Empty<IPackage>();
        }

        /// <summary>
        /// Adds a package to the registry.
        /// </summary>
        public void Add(IPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            IDictionary<SemanticVersion, IPackage> versions;
            if (!_packages.TryGetValue(package.Id, out versions))
                versions = new Dictionary<SemanticVersion, IPackage>();

            IPackage existingPackage;
            if (versions.TryGetValue(package.Version, out existingPackage))
                return;
            versions[package.Version] = package;
            _packages[package.Id] = versions;
        }

        /// <summary>
        /// Adds a package to the registry.
        /// </summary>
        public void Add(string packageId, SemanticVersion version)
        {
            Add(new NullPackage(packageId, version));
        }
    }
}