using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    public class NuPlugPackageManager : PackageManager
    {
#if DEBUG
        private static readonly Profiler InstallPackageProfiler = new Profiler("InstallPackages");
#endif

        public NuPlugPackageManager(IPackageRepository sourceRepository, string path)
            : base(sourceRepository, path)
        {
        }

        public bool DisableWalkInfo { get; set; } = true;

        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem) 
            : base(sourceRepository, pathResolver, fileSystem)
        {}

        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem, IPackageRepository localRepository) 
            : base(sourceRepository, pathResolver, fileSystem, localRepository)
        {}

        public FrameworkName TargetFramework { get; set; } = VersionHelper.GetTargetFramework();

        public override void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
#if DEBUG
            using (new ProfileMarker(InstallPackageProfiler))
#endif
                InstallPackage(package, TargetFramework, ignoreDependencies, allowPrereleaseVersions, DisableWalkInfo);
        }
    }
}