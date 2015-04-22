using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using NuGet;

namespace NuPlug
{
    public class PlugIns
    {
        private readonly IPackageManager _packageManager;
        private readonly AggregateCatalog _catalog;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        public readonly CompositionContainer Container;

        public event EventHandler Composed = (s,e) => {};

        public PlugIns(IPackageManager packageManager)
        {
            if (packageManager == null) throw new ArgumentNullException("packageManager");
            _packageManager = packageManager;
            _catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            Container = new CompositionContainer(_catalog);

            foreach (var package in _packageManager.LocalRepository.GetPackages())
                AddDirectoryCatalog(package);

            Container.ComposeParts(this);
            OnComposed();

            _packageManager.PackageInstalled += OnPackageInstalled;
            _packageManager.PackageUninstalled += OnPackageUninstalled;
        }

        private void OnPackageInstalled(object sender, PackageOperationEventArgs e)
        {
            AddDirectoryCatalog(e.Package);
            OnComposed();
        }

        private void OnPackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            var libDir = Path.Combine(e.InstallPath, "lib");
            foreach (var catalog in _catalog.Catalogs.OfType<DirectoryCatalog>()
                .Where(c => PathsMatch(c.FullPath, libDir)))
                _catalog.Catalogs.Remove(catalog);

            OnComposed();
        }

        private void AddDirectoryCatalog(IPackage package)
        {
            var libDir = Path.Combine(_packageManager.PathResolver.GetInstallPath(package), "lib");
            if (Directory.Exists(libDir)) 
                _catalog.Catalogs.Add(new DirectoryCatalog(libDir));
        }

        private void OnComposed() { Composed(this, EventArgs.Empty); }

        private static bool PathsMatch(string path1, string path2)
        {
            return String.Equals(
                path1.TrimEnd(Path.DirectorySeparatorChar), 
                path2.TrimEnd(Path.DirectorySeparatorChar), 
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
