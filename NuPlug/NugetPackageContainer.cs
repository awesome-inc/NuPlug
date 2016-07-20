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
        /// <summary>
        /// The underlying <see cref="IPackageManager"/>.
        /// </summary>
        public IPackageManager PackageManager { get; }

        /// <summary>
        /// The <see cref="FrameworkName"/>.
        /// </summary>
        public FrameworkName Framework { get; } = VersionHelper.GetTargetFramework();

        /// <summary>
        ///     Initializes a new instance of the <see cref="NuGetPackageContainer{TItem}" /> class.
        /// </summary>
        /// <param name="packageManager">The NuGet package manager to use. Usually a <see cref="NuPlugPackageManager"/></param>
        /// <param name="assemblyResolver">A custom <see cref="IResolveAssembly"/>. If null, uses the default <see cref="AssemblyResolver"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public NuGetPackageContainer(IPackageManager packageManager, IResolveAssembly assemblyResolver = null) : base(assemblyResolver) 
        {
            if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
            PackageManager = packageManager;

            foreach (var package in PackageManager.LocalRepository.GetPackages())
                AddPackageDirectory(package);

            PackageManager.PackageInstalled += OnPackageInstalled;
            PackageManager.PackageUninstalled += OnPackageUninstalled;
        }

        private void OnPackageInstalled(object sender, PackageOperationEventArgs e)
        {
            AddPackageDirectory(e.Package, e.FileSystem, e.InstallPath);
        }

        private void OnPackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            var libDir = Path.Combine(e.InstallPath, Constants.LibDirectory);
            RemoveDirectory(libDir);
        }

        private void AddPackageDirectory(IPackage package, IFileSystem fileSystem = null, string installPath = null)
        {
            var libDir = GetLibDir(package, fileSystem, installPath);
            if (!string.IsNullOrWhiteSpace(libDir))
                AddDirectory(libDir);
        }

        private string GetLibDir(IPackage package, IFileSystem fileSystem = null, string installPath = null)
        {
            var libDir = Path.Combine(installPath ?? PackageManager.PathResolver.GetInstallPath(package), Constants.LibDirectory);

            var fs = fileSystem ?? PackageManager.FileSystem;

            if (!fs.DirectoryExists(libDir))
                return null;

            if (fs.GetFiles("*.dll", libDir, false).Any())
                return libDir;

            var folderNames = fs.GetDirectories(libDir)
                .Select(p => p.Split(Path.DirectorySeparatorChar).Last())
                .ToList();

            var bestMatch = Framework.Version.BestMatch(folderNames);
            if (string.IsNullOrWhiteSpace(bestMatch)) return null;
            return Path.Combine(libDir, bestMatch);
        }
    }
}
