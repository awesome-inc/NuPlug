using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    public class NugetPackageContainer<TItem> 
        : PackageContainer<TItem> where TItem : class 
    {
        private readonly IPackageManager _packageManager;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly FrameworkName _framework = VersionHelper.GetTargetFramework();

        public NugetPackageContainer(IPackageManager packageManager) 
        {
            if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
            _packageManager = packageManager;

            foreach (var package in _packageManager.LocalRepository.GetPackages())
                AddDirectoryCatalog(package);

            _packageManager.PackageInstalled += OnPackageInstalled;
            _packageManager.PackageUninstalled += OnPackageUninstalled;
        }

        private void OnPackageInstalled(object sender, PackageOperationEventArgs e)
        {
            AddDirectoryCatalog(e.Package);
        }

        private void OnPackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            var libDir = Path.Combine(e.InstallPath, Constants.LibDirectory);
            RemoveDirectory(libDir);
        }

        private void AddDirectoryCatalog(IPackage package)
        {
            var libDir = Path.Combine(_packageManager.PathResolver.GetInstallPath(package), Constants.LibDirectory);
            var dirInfo = new DirectoryInfo(libDir);
            if (!dirInfo.Exists) return;

            if (dirInfo.GetFiles("*.dll", SearchOption.TopDirectoryOnly).Any())
            {
                AddDirectory(dirInfo.FullName);
            }
            else
            {
                var folderNames = dirInfo.GetDirectories().Select(d => d.Name);
                var bestMatch = _framework.Version.BestMatch(folderNames);
                if (string.IsNullOrWhiteSpace(bestMatch)) return;
                libDir = Path.Combine(dirInfo.FullName, bestMatch);
                AddDirectory(libDir);
            }
        }
    }
}
