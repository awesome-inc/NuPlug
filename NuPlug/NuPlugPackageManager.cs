using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// The NuPlug package manager
    /// </summary>
    public class NuPlugPackageManager : PackageManager, ISkipPackages
    {
#if DEBUG
        private static readonly Profiler InstallPackageProfiler = new Profiler("InstallPackages");
#endif

        /// <summary>
        ///     Initializes a new instance of the <see cref="NuPlugPackageManager" /> class.
        /// </summary>
        /// <param name="sourceRepository">The source repository</param>
        /// <param name="path">The local package path. Can be relative</param>
        public NuPlugPackageManager(IPackageRepository sourceRepository, string path)
            : this(sourceRepository, new DefaultPackagePathResolver(Assemblies.GetFullPath(path)), new PhysicalFileSystem(Assemblies.GetFullPath(path)))
        {
            CheckDowngrade = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NuPlugPackageManager" /> class.
        /// </summary>
        /// <param name="sourceRepository"></param>
        /// <param name="pathResolver"></param>
        /// <param name="fileSystem"></param>
        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem)
            : this(sourceRepository, pathResolver, fileSystem, new NuPlugPackageRepository(new LocalPackageRepository(pathResolver, fileSystem)))
        {}

        /// <summary>
        ///     Initializes a new instance of the <see cref="NuPlugPackageManager" /> class.
        /// </summary>
        /// <param name="sourceRepository"></param>
        /// <param name="pathResolver"></param>
        /// <param name="fileSystem"></param>
        /// <param name="localRepository"></param>
        public NuPlugPackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem, IPackageRepository localRepository)
            : base(sourceRepository, pathResolver, fileSystem, localRepository)
        { }

        /// <summary>
        /// If true (default), skips walking info. This significantly speeds up package installation.
        /// </summary>
        public bool DisableWalkInfo { get; set; } = true;

        /// <summary>
        /// The target framework to install packages for.
        /// This significantly reduces installation time and traffic as well as the risk of unsatisfieable dependencies.
        /// Defaults to the framework version of the current process at runtime.
        /// </summary>
        public FrameworkName TargetFramework { get; set; } = VersionHelper.GetTargetFramework();

        /// <summary>
        /// Installs the specified package
        /// </summary>
        /// <param name="package">The package to install</param>
        /// <param name="ignoreDependencies">If false, also recursively installs all dependencies</param>
        /// <param name="allowPrereleaseVersions">If true, allows prerelase versions</param>
        public override void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
#if DEBUG
            using (new ProfileMarker(InstallPackageProfiler))
#endif
            {
                try
                {
                    InstallPackage(package, TargetFramework, ignoreDependencies, allowPrereleaseVersions, DisableWalkInfo);
                }
                catch (Exception ex)
                {
                    Logger.Log(MessageLevel.Warning, $"Could not install '{package.Id} {package.Version}': {ex}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Advise to skip resolving the specified packages when installing/restoring NuGet packages using an <see cref="IPackageManager"/>.
        /// </summary>
        /// <param name="packages"></param>
        public void SkipPackages(IEnumerable<IPackageName> packages)
        {
            var skipPackages = LocalRepository as ISkipPackages;
            if (skipPackages == null) throw new InvalidOperationException("Cannot skip packages on this local repository");
            skipPackages.SkipPackages(packages);
        }

        internal void SetLocalRepository(IPackageRepository repository)
        {
            var p = typeof(PackageManager).GetProperty(nameof(LocalRepository), BindingFlags.Instance | BindingFlags.Public);
            p.SetValue(this, repository);
        }
    }
}