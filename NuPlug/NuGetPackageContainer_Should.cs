using System.IO;
using System.Linq;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(NuGetPackageContainer<>))]
    // ReSharper disable once InconsistentNaming
    internal class NuGetPackageContainer_Should
    {
        [Test, Issue("https://github.com/awesome-inc/NuPlug/issues/11")]
        public void Support_accessing_the_package_manager()
        {
            var context = new ContextFor<NuGetPackageContainer<string>>();
            var sut = context.BuildSut();

            sut.PackageManager.Should().NotBeNull();
            sut.Framework.Should().Be(VersionHelper.GetTargetFramework());
        }

        [Test]
        public void Sync_directories_with_installed_packages()
        {
            var context = new ContextFor<NuGetPackageContainer<string>>();

            var foo = "foo".CreatePackage("0.1.0");
            var packages = new[] {foo};

            var pm = context.For<IPackageManager>();
            pm.LocalRepository.GetPackages().Returns(packages.AsQueryable());

            var path = foo.Id;
            pm.PathResolver.GetInstallPath(foo).Returns(path);
            var expectedPath = MockFileSystem(pm.FileSystem, foo);

            var sut = context.BuildSut();

            pm.LocalRepository.Received().GetPackages();
            var asmResolver = context.For<IResolveAssembly>();
            asmResolver.Received().Directories.Add(expectedPath);

            var bar = "bar".CreatePackage("1.42.0");
            var fs2 = Substitute.For<IFileSystem>();
            expectedPath = MockFileSystem(fs2, bar);
            var args = new PackageOperationEventArgs(bar, fs2, "otherPath");

            pm.PackageInstalled += Raise.EventWith(args);

            asmResolver.Received().Directories.Add(expectedPath);

            pm.PackageUninstalled += Raise.EventWith(args);
            asmResolver.Received().Directories.Remove(expectedPath);
        }

        private static string MockFileSystem(IFileSystem fileSystem, IPackage package)
        {
            var path = package.Id;
            var libDir = Path.Combine(path, Constants.LibDirectory);

            fileSystem.DirectoryExists(libDir).Returns(true);
            var folders = new[] { "net45" };
            fileSystem.GetDirectories(libDir).Returns(folders);

            var assemblies = new[] { $"{package.Id}.dll" };
            fileSystem.GetFiles(libDir, "*.dll", false).Returns(assemblies);

            return Path.Combine(path, Constants.LibDirectory, "net45");
        }
    }
}