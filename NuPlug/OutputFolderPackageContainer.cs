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
    public class OutputFolderPackageContainer<TItem> : IPackageContainer<TItem> where TItem : class
    {
        private readonly AggregateCatalog _catalog;

        public OutputFolderPackageContainer(string localFolder)
        {
            Items = new ObservableCollection<TItem>();

            _catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetEntryAssembly()));
            var container = new CompositionContainer(_catalog);
            var dirs = Directory.GetDirectories(localFolder).ToList();
            dirs.ForEach(libDir => _catalog.Catalogs.Add(new DirectoryCatalog(libDir)));

            container.ComposeParts(this);
            OnComposed();
        }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<TItem> Items { get; private set; }

        private void OnComposed() { Composed(this, EventArgs.Empty); }

        public event EventHandler Composed = (s, e) => { };
    }
}