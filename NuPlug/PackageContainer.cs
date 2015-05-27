﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

namespace NuPlug
{
#if DEBUG
    [NEdifis.Attributes.TestedBy(typeof(PackageContainer_Should))]
#endif
    public class PackageContainer<TItem>
        : IPackageContainer<TItem>
        , IDisposable
        where TItem : class
    {
        public event EventHandler Updated;
        public readonly CompositionBatch Batch = new CompositionBatch();
        public readonly AggregateCatalog Catalog = new AggregateCatalog();
        public readonly CompositionContainer Container;

        public PackageContainer()
        {
            Batch.AddPart(this);
            Container = new CompositionContainer(Catalog);
            Items = new ObservableCollection<TItem>();
        }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<TItem> Items { get; private set; }

        public virtual void Update()
        {
            Container.Compose(Batch);
            OnComposed();
        }

        public virtual void Dispose()
        {
            Container.Dispose();
        }

        public virtual void AddDirectory(string libDir)
        {
            if (CatalogsMatching(libDir).Any())
                return;

            using (new AssemblyResolver(libDir))
            {
                Catalog.Catalogs.Add(new DirectoryCatalog(libDir));
                OnComposed();
            }
        }

        public virtual void RemoveDirectory(string directory)
        {
            foreach (var catalog in CatalogsMatching(directory))
                Catalog.Catalogs.Remove(catalog);
            OnComposed();
        }

        private void OnComposed() { Updated?.Invoke(this, EventArgs.Empty); }

        private IEnumerable<DirectoryCatalog> CatalogsMatching(string directory)
        {
            var trimmed = directory.TrimEnd(Path.DirectorySeparatorChar);
            return Catalog.Catalogs
                .OfType<DirectoryCatalog>()
                .Where(c => c.FullPath.StartsWith(trimmed))
                .ToArray(); // NOTE: don't be lazy here to avoid 'collection modified'-errors
        }
    }
}