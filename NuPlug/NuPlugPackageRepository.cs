using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuPlug
{
    public class NuPlugPackageRepository : IPackageLookup
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IPackageLookup _packageLookup;
        private readonly IPackageRegistry _packageRegistry;


        public NuPlugPackageRepository(IPackageLookup packageLookup, IPackageRegistry packageRegistry = null)
        {
            if (packageLookup == null) throw new ArgumentNullException(nameof(packageLookup));
            _packageLookup = packageLookup;
            _packageRepository = packageLookup;
            _packageRegistry = packageRegistry ?? new NuPlugPackageRegistry();
        }

        public IQueryable<IPackage> GetPackages()
        {
            return _packageRepository.GetPackages();
        }

        public void AddPackage(IPackage package)
        {
            _packageRepository.AddPackage(package);
        }

        public void RemovePackage(IPackage package)
        {
            _packageRepository.RemovePackage(package);
        }

        public string Source => _packageRepository.Source;

        public PackageSaveModes PackageSaveMode
        {
            get { return _packageRepository.PackageSaveMode; }
            set { _packageRepository.PackageSaveMode = value; }
        }

        public bool SupportsPrereleasePackages => _packageRepository.SupportsPrereleasePackages;

        public bool Exists(string packageId, SemanticVersion version)
        {
            return _packageRegistry.Exists(packageId, version) || _packageLookup.Exists(packageId, version);
        }

        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            var package = _packageRegistry.FindPackage(packageId, version);
            return package ?? _packageLookup.FindPackage(packageId, version);
        }

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            var packages = _packageRegistry.FindPackagesById(packageId).ToList();
            return packages.Any() ? packages : _packageLookup.FindPackagesById(packageId);
        }
    }
}