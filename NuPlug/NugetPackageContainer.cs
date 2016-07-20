using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// The generic NuPlug <see cref="IPackageContainer{TItem}"/> using MEF composition to resolve items/plugins from NuGet packages.
    /// </summary>
    public class NuGetPackageContainer<TItem> 
        : PackageContainer<TItem> where TItem : class 
    {
        private readonly IPackageManager _packageManager;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly FrameworkName _framework = VersionHelper.GetTargetFramework();

        /// <summary>
        ///     Initializes a new instance of the <see cref="NuGetPackageContainer{TItem}" /> class.
        /// </summary>
        /// <param name="packageManager">The NuGet package manager to use. Usually a <see cref="NuPlugPackageManager"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NuGetPackageContainer(IPackageManager packageManager) 
        {
            if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
            _packageManager = packageManager;

            foreach (var package in _packageManager.LocalRepository.GetPackages())
                AddPackageDirectory(package);

            _packageManager.PackageInstalled += OnPackageInstalled;
            _packageManager.PackageUninstalled += OnPackageUninstalled;
        }

        private void OnPackageInstalled(object sender, PackageOperationEventArgs e)
        {
            AddPackageDirectory(e.Package);
        }

        private void OnPackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            var libDir = Path.Combine(e.InstallPath, Constants.LibDirectory);
            RemoveDirectory(libDir);
        }

        private void AddPackageDirectory(IPackage package)
        {
            var libDir = GetLibDir(package);
            if (!string.IsNullOrWhiteSpace(libDir))
                AddDirectory(libDir);
        }

        private string GetLibDir(IPackage package)
        {
            var libDir = Path.Combine(_packageManager.PathResolver.GetInstallPath(package), Constants.LibDirectory);
            var dirInfo = new DirectoryInfo(libDir);
            if (!dirInfo.Exists) return null;

            if (dirInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Any())
                return dirInfo.FullName;

            var folderNames = dirInfo.GetDirectories().Select(d => d.Name);
            var bestMatch = _framework.Version.BestMatch(folderNames);
            if (string.IsNullOrWhiteSpace(bestMatch)) return null;
            return Path.Combine(dirInfo.FullName, bestMatch);
        }
    }
}
