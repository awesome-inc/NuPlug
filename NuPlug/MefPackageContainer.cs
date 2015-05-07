using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace NuPlug
{
    public class MefPackageContainer<TItem> : IPackageContainer<TItem> where TItem : class
    {
        private readonly AggregateCatalog _catalog;

        public MefPackageContainer(CompositionContainer container)
        {
            Items = new ObservableCollection<TItem>();

            container.ComposeParts(this);
            OnComposed();
        }

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<TItem> Items { get; private set; }

        private void OnComposed() { Composed(this, EventArgs.Empty); }

        public event EventHandler Composed = (s, e) => { };
    }
}