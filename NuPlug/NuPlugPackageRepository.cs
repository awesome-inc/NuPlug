using System;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// A custom package repository decorating <see cref="IPackageLookup"/> that supports <see cref="ISkipPackages"/>.
    /// </summary>
    public class NuPlugPackageRepository : IPackageLookup, ISkipPackages
    {
        private readonly IPackageLookup _packageLookup;
        private readonly IPackageRegistry _packageRegistry;


        /// <summary>
        ///     Initializes a new instance of the <see cref="NuPlugPackageManager" /> class.
        /// </summary>
        /// <param name="packageLookup">The <see cref="IPackageLookup"/> to be wrapped.</param>
        /// <param name="packageRegistry">The <see cref="IPackageRegistry"/> to use for mocking existing packages at runtime</param>
        /// <exception cref="ArgumentNullException"></exception>
        public NuPlugPackageRepository(IPackageLookup packageLookup, IPackageRegistry packageRegistry = null)
        {
            if (packageLookup == null) throw new ArgumentNullException(nameof(packageLookup));
            _packageLookup = packageLookup;
            _packageRegistry = packageRegistry ?? new NuPlugPackageRegistry();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<IPackage> GetPackages()
        {
            return _packageLookup.GetPackages();
        }

        /// <summary>
        /// Adds the specified <see cref="IPackage"/> to the repository.
        /// If this is a <see cref="NullPackage"/> it is only added to the internal <see cref="IPackageRegistry"/>.
        /// </summary>
        public void AddPackage(IPackage package)
        {
            if (package is NullPackage)
                _packageRegistry.Add(package);
            else
                _packageLookup.AddPackage(package);
        }

        /// <summary>
        /// Removes the specified <see cref="IPackage"/> from the internally wrapped <see cref="IPackageLookup"/>.
        /// </summary>
        public void RemovePackage(IPackage package)
        {
            _packageLookup.RemovePackage(package);
        }

        /// <summary>
        /// The package source (wrapped).
        /// </summary>
        public string Source => _packageLookup.Source;

        /// <summary>
        /// The package save mode (wrapped).
        /// </summary>
        public PackageSaveModes PackageSaveMode
        {
            get { return _packageLookup.PackageSaveMode; }
            set { _packageLookup.PackageSaveMode = value; }
        }

        /// <summary>
        /// If true, supports pre-release packages (wrapped).
        /// </summary>
        public bool SupportsPrereleasePackages => _packageLookup.SupportsPrereleasePackages;

        /// <summary>
        /// Checks whether a package exists in the wrapped <see cref="IPackageRegistry"/> or <see cref="IPackageLookup"/>.
        /// </summary>
        /// <returns>True, if found; False, otherwise.</returns>
        public bool Exists(string packageId, SemanticVersion version)
        {
            return _packageRegistry.Exists(packageId, version) || _packageLookup.Exists(packageId, version);
        }

        /// <summary>
        /// Finds a package in the wrapped <see cref="IPackageRegistry"/> or <see cref="IPackageLookup"/>.
        /// As this is used to "fake" the existence of certain packages, the resulting package can be of type <see cref="NullPackage"/>.
        /// </summary>
        /// <returns>A valid <see cref="IPackage"/> if found; null, otherwise.</returns>
        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            var package = _packageRegistry.FindPackage(packageId, version);
            return package ?? _packageLookup.FindPackage(packageId, version);
        }

        /// <summary>
        /// Finds all packages for the specified <paramref name="packageId"/> in the wrapped <see cref="IPackageRegistry"/> or <see cref="IPackageLookup"/>.
        /// </summary>
        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            var packages = _packageRegistry.FindPackagesById(packageId).ToList();
            return packages.Any() ? packages : _packageLookup.FindPackagesById(packageId);
        }

        /// <summary>
        /// Advise to skip resolving the specified packages when installing/restoring NuGet packages using an <see cref="IPackageManager"/>.
        /// </summary>
        /// <param name="packages"></param>
        public void SkipPackages(IEnumerable<IPackageName> packages)
        {
            packages.ToList().ForEach(p => _packageRegistry.Add(p.Id, p.Version));
        }
    }
}