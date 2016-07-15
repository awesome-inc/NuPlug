using System.Collections.Generic;
using NuGet;

namespace NuPlug
{
    public interface IPackageRegistry
    {
        bool Exists(string packageId, SemanticVersion version);

        IPackage FindPackage(string packageId, SemanticVersion version);

        IEnumerable<IPackage> FindPackagesById(string packageId);

        void Add(string packageId, SemanticVersion version);

        void Add(IPackage package);
    }
}