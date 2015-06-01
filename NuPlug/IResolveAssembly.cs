using System;
using System.Collections.Generic;

namespace NuPlug
{
    public interface IResolveAssembly : IDisposable
    {
        IList<string> Directories { get; } 
    }
}