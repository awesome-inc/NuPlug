using System;
using System.Collections.Generic;

namespace NuPlug
{
    /// <summary>
    ///     A package container to retreive plugins.
    /// </summary>
    /// <typeparam name="TItem">The type of items (plugin) to manage.</typeparam>
    public interface IPackageContainer<out TItem> where TItem : class
    {
        /// <summary>
        /// The resolved items (plugins).
        /// </summary>
        IEnumerable<TItem> Items { get; }

        /// <summary>
        /// Refresh the <see cref="Items"/> collection. Usually triggers a recomposition of the underlying IoC container (e.g. MEF).
        /// </summary>
        void Update();
        
        /// <summary>
        /// Raised after the <see cref="Items"/> collection changed due to a call to <see cref="Update"/>.
        /// </summary>
        event EventHandler Updated;
    }
}