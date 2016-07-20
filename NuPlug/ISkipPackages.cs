using System.Collections.Generic;
using NuGet;

namespace NuPlug
{
    public interface ISkipPackages
    {
        void SkipPackages(IEnumerable<IPackageName> packages);
    }
}