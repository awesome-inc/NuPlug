using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuPlug
{
    public class NuPlugPackageRepository : IPackageLookup, ISkipPackages
    {
        private readonly IPackageLookup _packageLookup;
        private readonly IPackageRegistry _packageRegistry;


        public NuPlugPackageRepository(IPackageLookup packageLookup, IPackageRegistry packageRegistry = null)
        {
            if (packageLookup == null) throw new ArgumentNullException(nameof(packageLookup));
            _packageLookup = packageLookup;
            _packageRegistry = packageRegistry ?? new NuPlugPackageRegistry();
        }

        public IQueryable<IPackage> GetPackages()
        {
            return _packageLookup.GetPackages();
        }

        public void AddPackage(IPackage package)
        {
            if (package is NullPackage)
                _packageRegistry.Add(package);
            else
                _packageLookup.AddPackage(package);
        }

        public void RemovePackage(IPackage package)
        {
            _packageLookup.RemovePackage(package);
        }

        public string Source => _packageLookup.Source;

        public PackageSaveModes PackageSaveMode
        {
            get { return _packageLookup.PackageSaveMode; }
            set { _packageLookup.PackageSaveMode = value; }
        }

        public bool SupportsPrereleasePackages => _packageLookup.SupportsPrereleasePackages;

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

        public void SkipPackages(IEnumerable<IPackageName> packages)
        {
            packages.ToList().ForEach(p => _packageRegistry.Add(p.Id, p.Version));
        }
    }
}