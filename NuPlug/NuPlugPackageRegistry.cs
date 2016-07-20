using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NuGet;

namespace NuPlug
{
    public class NuPlugPackageRegistry : IPackageRegistry
    {
        private static readonly string AppDir = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetDirectory();

        private readonly IDictionary<string, IDictionary<SemanticVersion, IPackage>> _packages =
            new Dictionary<string, IDictionary<SemanticVersion, IPackage>>();

        public bool Exists(string packageId, SemanticVersion version = null)
        {
            IDictionary<SemanticVersion, IPackage> versions;
            if (_packages.TryGetValue(packageId, out versions))
            {
                return version == null || versions.ContainsKey(version);
            }

            var libFile = Path.Combine(AppDir, $"{packageId}.dll");
            return File.Exists(libFile);
        }

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

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId)) throw new ArgumentNullException(nameof(packageId));
            IDictionary<SemanticVersion, IPackage> versions;
            if (_packages.TryGetValue(packageId, out versions))
                return versions.Values.ToList();
            return Enumerable.Empty<IPackage>();
        }

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

        public void Add(string packageId, SemanticVersion version)
        {
            Add(new NullPackage(packageId, version));
        }
    }
}