using System;
using System.Collections.Generic;

namespace NuPlug
{
    /// <summary>
    /// Resolves assemblies from custom directories, e.g. plugin or nuget package directories.
    /// </summary>
    public interface IResolveAssembly : IDisposable
    {
        /// <summary>
        /// The directories to check when resolving assemblies.
        /// </summary>
        IList<string> Directories { get; } 
    }
}