using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    public class NuPlugPackageManager : PackageManager, ISkipPackages
    {
#if DEBUG
        private static readonly Profiler InstallPackageProfiler = new Profiler("InstallPackages");
#endif

        public NuPlugPackageManager(IPackageRepository sourceRepository, string path)
            : this(sourceRepository, new DefaultPackagePathResolver(path), new PhysicalFileSystem(path))
        {
            CheckDowngrade = false;
        }

        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : this(sourceRepository, pathResolver, fileSystem, new NuPlugPackageRepository(new LocalPackageRepository(pathResolver, fileSystem)))
        {}

        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem, IPackageRepository localRepository)
            : base(sourceRepository, pathResolver, fileSystem, localRepository)
        { }

        public bool DisableWalkInfo { get; set; } = true;

        public FrameworkName TargetFramework { get; set; } = VersionHelper.GetTargetFramework();

        public override void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
#if DEBUG
            using (new ProfileMarker(InstallPackageProfiler))
#endif
                InstallPackage(package, TargetFramework, ignoreDependencies, allowPrereleaseVersions, DisableWalkInfo);
        }

        internal void SetLocalRepository(IPackageRepository repository)
        {
            var p = typeof(PackageManager).GetProperty(nameof(LocalRepository), BindingFlags.Instance | BindingFlags.Public);
            p.SetValue(this, repository);
        }

        public void SkipPackages(IEnumerable<IPackageName> packages)
        {
            var skipPackages = LocalRepository as ISkipPackages;
            if (skipPackages == null) throw new InvalidOperationException("Cannot skip packages on this local repository");
            skipPackages.SkipPackages(packages);
        }
    }
}