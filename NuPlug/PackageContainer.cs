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
    /// <summary>
    /// A generic <see cref="IPackageContainer{TItem}"/> using MEF composition to resolve items/plugins.
    /// </summary>
    /// <typeparam name="TItem">The type of items (plugin) to manage.</typeparam>
    public class PackageContainer<TItem>
        : IPackageContainer<TItem>
        , IDisposable
        where TItem : class
    {
        private readonly AggregateCatalog _catalog = new AggregateCatalog();
        private readonly CompositionContainer _container;
        private readonly IResolveAssembly _assemblyResolver;
        private Func<Type, bool> _typeFilter = type => IsPublicImplementationOf(type);
        private Func<string, bool> _fileFilter = fileName => fileName.EndsWith(".dll");

        /// <summary>
        ///     Initializes a new instance of the <see cref="PackageContainer{TItem}" /> class.
        /// </summary>
        /// <param name="assemblyResolver">A custom <see cref="IResolveAssembly"/>. If null, uses the default <see cref="AssemblyResolver"/>.</param>
        public PackageContainer(IResolveAssembly assemblyResolver = null)
        {
            _assemblyResolver = assemblyResolver ?? new AssemblyResolver();
            Batch.AddPart(this);
            _container = new CompositionContainer(_catalog);
        }


        /// <summary>
        /// Raised after the <see cref="IPackageContainer{TItem}.Items"/> collection changed due to a call to <see cref="IPackageContainer{TItem}.Update"/>.
        /// </summary>
        public event EventHandler Updated;

        /// <summary>
        /// The MEF <see cref="CompositionBatch"/>. Can be used to customize composition in special scenarios, e.g. when some implementations of <typeparamref name="TItem"/>
        /// have dependencies you can register an instance (i.e. add an export) before composing the batch to satisfy those dependencies.
        /// </summary>
        public CompositionBatch Batch { get; } = new CompositionBatch();

        /// <summary>
        /// A predicate that can be used to restrict loading to specific types.
        /// The default is to load only public, non-abstract implementations of <typeparamref name="TItem"/> 
        /// as this reduces the chance of any <see cref="TypeLoadException"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<Type, bool> TypeFilter
        {
            get { return _typeFilter; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), $"'{nameof(TypeFilter)}' must not be null.");
                _typeFilter = value;
            }
        }

        /// <summary>
        /// A predicate that can be used to restrict loading to specific assemblies (filenames).
        /// The default is to load files ending with ".dll" as this reduces resolving time e.g. when using packages with large content.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Func<string, bool> FileFilter
        {
            get { return _fileFilter; }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), $"'{nameof(FileFilter)}' must not be null.");
                _fileFilter = value;
            }
        }


        /// <summary>
        /// The reflection context to use on composition. Can be used customize registration scenarios, e.g. 
        /// using MEF conventions with <a href="https://msdn.microsoft.com/en-us/library/system.componentmodel.composition.registration.registrationbuilder(v=vs.110).aspx">RegistrationBuilder</a>.
        /// </summary>
        public ReflectionContext Conventions { get; set; }

        /// <summary>
        /// The resolved items (plugins).
        /// </summary>
        [ImportMany(AllowRecomposition = true)]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public IEnumerable<TItem> Items { get; private set; } = new ObservableCollection<TItem>();

        /// <summary>
        /// Refresh the <see cref="IPackageContainer{TItem}.Items"/> collection. Usually triggers a recomposition of the underlying IoC container (e.g. MEF).
        /// </summary>
        public virtual void Update()
        {
            SyncCatalogs();
            _container.Compose(Batch);
            Updated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            _container.Dispose();
            _assemblyResolver.Dispose();
        }

        /// <summary>
        /// Adds a directory to resolve assemblies from.
        /// </summary>
        protected internal void AddDirectory(string libDir)
        {
            if (_assemblyResolver.Directories.Contains(libDir))
                return;
            _assemblyResolver.Directories.Add(libDir);
        }

        /// <summary>
        /// Removes a library directory.
        /// </summary>
        protected internal void RemoveDirectory(string libDir)
        {
            if (_assemblyResolver.Directories.Contains(libDir))
                _assemblyResolver.Directories.Remove(libDir);
        }

        private static bool IsPublicImplementationOf(Type type)
        {
            return type.IsPublic && type.IsClass && !type.IsAbstract && typeof(TItem).IsAssignableFrom(type);
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
            var catalog = new SafeDirectoryCatalog(libDir, TypeFilter, Conventions, FileFilter);
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