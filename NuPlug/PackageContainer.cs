using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NuPlug
{
    public class PackageContainer<TItem>
        : IPackageContainer<TItem>
        , IDisposable
        where TItem : class
    {
        private readonly AggregateCatalog _catalog = new AggregateCatalog();
        private readonly CompositionContainer _container;
        private readonly IResolveAssembly _assemblyResolver;
        private Func<Type, bool> _typeFilter = type => IsPublicImplementationOf(type);

        public event EventHandler Updated;
        public CompositionBatch Batch { get; } = new CompositionBatch();

        public Func<Type, bool> TypeFilter
        {
            get { return _typeFilter; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), $"'{nameof(TypeFilter)}' must not be null.");
                _typeFilter = value;
            }
        }

        public ReflectionContext Conventions { get; set; }

        private static bool IsPublicImplementationOf(Type type)
        {
            return type.IsPublic && type.IsClass && !type.IsAbstract && typeof(TItem).IsAssignableFrom(type);
        }

        public PackageContainer(IResolveAssembly assemblyResolver = null)
        {
            _assemblyResolver = assemblyResolver ?? new AssemblyResolver();
            Batch.AddPart(this);
            _container = new CompositionContainer(_catalog);
        }

        [ImportMany(AllowRecomposition = true)]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<TItem> Items { get; private set; } = new ObservableCollection<TItem>();

        public virtual void Update()
        {
            SyncCatalogs();
            _container.Compose(Batch);
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public virtual void Dispose()
        {
            _container.Dispose();
            _assemblyResolver.Dispose();
        }

        protected internal void AddDirectory(string libDir)
        {
            if (_assemblyResolver.Directories.Contains(libDir))
                return;
            _assemblyResolver.Directories.Add(libDir);
        }

        protected internal void RemoveDirectory(string libDir)
        {
            if (_assemblyResolver.Directories.Contains(libDir))
                _assemblyResolver.Directories.Remove(libDir);
        }

        private void SyncCatalogs()
        {
            // remove obsolete
            var dirsToRemove = _catalog.Catalogs.OfType<SafeDirectoryCatalog>().Select(c => c.FullPath)
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
            var catalog = new SafeDirectoryCatalog(libDir, TypeFilter, Conventions);
            _catalog.Catalogs.Add(catalog);
        }

        private void RemoveCatalogsFor(string directory)
        {
            foreach (var catalog in CatalogsMatching(directory))
                _catalog.Catalogs.Remove(catalog);
        }

        private IEnumerable<SafeDirectoryCatalog> CatalogsMatching(string directory)
        {
            var trimmed = directory.TrimEnd(Path.DirectorySeparatorChar);
            return _catalog.Catalogs
                .OfType<SafeDirectoryCatalog>()
                .Where(c => c.FullPath.StartsWith(trimmed))
                .ToArray(); // NOTE: don't be lazy here to avoid 'collection modified'-errors
        }
    }
}