using System.Collections.Generic;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// A strategy interface to skip certain packages at runtime. Imlpemented by <see cref="NuPlugPackageManager"/> and <see cref="NuPlugPackageRepository"/>.
    /// </summary>
    public interface ISkipPackages
    {
        /// <summary>
        /// Advise to skip resolving the specified packages when installing/restoring NuGet packages using an <see cref="IPackageManager"/>.
        /// </summary>
        /// <param name="packages"></param>
        void SkipPackages(IEnumerable<IPackageName> packages);
    }
}