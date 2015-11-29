using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

namespace NuPlug
{
    public class PackageContainer<TItem>
        : IPackageContainer<TItem>
        , IDisposable
        where TItem : class
    {
        private readonly IResolveAssembly _assemblyResolver;
        public event EventHandler Updated;
        private readonly AggregateCatalog _catalog = new AggregateCatalog();
        private readonly CompositionContainer _container;

        // ReSharper disable once InconsistentNaming
        internal readonly CompositionBatch _batch = new CompositionBatch();

        public PackageContainer(IResolveAssembly assemblyResolver = null)
        {
            _assemblyResolver = assemblyResolver ?? new AssemblyResolver();
            _batch.AddPart(this);
            _container = new CompositionContainer(_catalog);
            Items = new ObservableCollection<TItem>();
        }

        [ImportMany(AllowRecomposition = true)]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<TItem> Items { get; private set; }

        public virtual void Update()
        {
            SyncCatalogs();
            _container.Compose(_batch);
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            _container.Dispose();
            _assemblyResolver.Dispose();
        }

        protected void AddDirectory(string libDir)
        {
            if (_assemblyResolver.Directories.Contains(libDir))
                return;
            _assemblyResolver.Directories.Add(libDir);
        }

        protected void RemoveDirectory(string libDir)
        {
            if (_assemblyResolver.Directories.Contains(libDir))
                _assemblyResolver.Directories.Remove(libDir);
        }

        private void SyncCatalogs()
        {
            // remove obsolete
            var dirsToRemove = _catalog.Catalogs.OfType<DirectoryCatalog>().Select(c => c.FullPath)
                .Except(_assemblyResolver.Directories);
            foreach (var directory in dirsToRemove)
                RemoveCatalogsFor(directory);

            // add new
            foreach (var directory in _assemblyResolver.Directories)
                AddCatalogFor(directory);
        }

        private void AddCatalogFor(string libDir)
        {
            if (CatalogsMatching(libDir).Any())
                return;
            var catalog = new DirectoryCatalog(libDir);
            _catalog.Catalogs.Add(catalog);
        }

        private void RemoveCatalogsFor(string directory)
        {
            foreach (var catalog in CatalogsMatching(directory))
                _catalog.Catalogs.Remove(catalog);
        }

        private IEnumerable<DirectoryCatalog> CatalogsMatching(string directory)
        {
            var trimmed = directory.TrimEnd(Path.DirectorySeparatorChar);
            return _catalog.Catalogs
                .OfType<DirectoryCatalog>()
                .Where(c => c.FullPath.StartsWith(trimmed))
                .ToArray(); // NOTE: don't be lazy here to avoid 'collection modified'-errors
        }
    }
}