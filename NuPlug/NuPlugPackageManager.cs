using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    public class NuPlugPackageManager : PackageManager
    {
        public NuPlugPackageManager(IPackageRepository sourceRepository, string path) 
            : base(sourceRepository, path)
        {}

        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem) 
            : base(sourceRepository, pathResolver, fileSystem)
        {}

        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem, IPackageRepository localRepository) 
            : base(sourceRepository, pathResolver, fileSystem, localRepository)
        {}

        public FrameworkName TargetFramework { get; set; } = VersionHelper.GetTargetFramework();

        //public override void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        //{
        //    InstallPackage(package, TargetFramework, ignoreDependencies, allowPrereleaseVersions);
        //}
    }
}