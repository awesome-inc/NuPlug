using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NEdifis.Diagnostics;
using NSubstitute;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(NuPlugPackageManager))]
    // ReSharper disable once InconsistentNaming
    internal class NuPlugPackageManager_Should
    {
        [Test]
        public void Override_all_base_ctors()
        {
            // ReSharper disable ObjectCreationAsStatement
            0.Invoking(x => new NuPlugPackageManager(null, null, null,null)).ShouldThrow<ArgumentNullException>();
            1.Invoking(x => new NuPlugPackageManager(null,null,null)).ShouldThrow<ArgumentNullException>();
            2.Invoking(x => new NuPlugPackageManager(null, string.Empty)).ShouldThrow<ArgumentException>();
            // ReSharper restore ObjectCreationAsStatement

            var repo = Substitute.For<IPackageRepository>();
            var sut = new NuPlugPackageManager(repo, "a");
            sut.SourceRepository.Should().Be(repo);
            sut.LocalRepository.Should().BeOfType<NuPlugPackageRepository>();

            sut.CheckDowngrade.Should().BeFalse("We explicitly don't want downgrade functionality.");
        }

        [Test, Issue("https://github.com/awesome-inc/NuPlug/issues/10", Title= "Speedup local packages")]
        public void Ignore_Walk_info_by_default()
        {
            var context = new ContextFor<NuPlugPackageManager>();
            var sut = context.BuildSut();

            sut.DisableWalkInfo.Should().BeTrue();

            var package = "foo".CreatePackage("0.1.0");
            sut.Logger = new TraceLogger();

            using (var tl = new TestTraceListener())
            {
                sut.InstallPackage(package, false, true);
                var infos = tl.MessagesFor(TraceLevel.Info);
                infos.Should().BeEquivalentTo(
                    $"Installing '{package.Id} {package.Version}'.",
                    $"Successfully installed '{package.Id} {package.Version}'.");
            }
        }

        [Test]
        [Issue("#6", Title = "Installing packages should respect the requested target framework")]
        [TestCase("net452")]
        [TestCase("net40")]
        [TestCase("net35")]
        [TestCase("net20")]
        [TestCase(".NETFramework4.5")]
        [TestCase("DNX4.5.1")]
        [TestCase("DNXCore5.0")]
        public void Use(string frameworkName)
        {
            var context = new ContextFor<NuPlugPackageManager>();
            var sut = context.BuildSut();

            sut.TargetFramework.Should().Be(VersionHelper.GetTargetFramework());

            var targetFramework = VersionUtility.ParseFrameworkName(frameworkName);
            targetFramework.Should().NotBeNull();

            sut.TargetFramework = targetFramework;
            sut.TargetFramework.Should().Be(targetFramework);
        }

        [Test]
        [TestCase(".NETFramework4.5", "DNXCore5.0")]
        public void Only_install_dependencies_matching_target_framework(string thisGroup, string otherGroup)
        {
            var context = new ContextFor<NuPlugPackageManager>();
            var sut = context.BuildSut();

            var targetFramework = VersionUtility.ParseFrameworkName(thisGroup);
            var otherFramework = VersionUtility.ParseFrameworkName(otherGroup);

            var package = "foo".CreatePackage("0.1.0");
            var dependency = "bar".CreatePackage("0.1.1");
            var otherDependency = "buzz".CreatePackage("0.1.2");
            var deps = new[]
            {
                new PackageDependencySet(targetFramework, new [] {new PackageDependency(dependency.Id) }),
                new PackageDependencySet(otherFramework, new [] {new PackageDependency(otherDependency.Id) })
            };
            package.DependencySets.Returns(deps);

            var localPackages = Enumerable.Empty<IPackage>();
            sut.LocalRepository.GetPackages().Returns(localPackages.AsQueryable());
            var remotePackages = new[] { package, dependency, otherDependency };
            sut.SourceRepository.GetPackages().Returns(remotePackages.AsQueryable());

            //repo
            sut.Logger = new TraceLogger();

            using (var ttl = new TestTraceListener())
            {
                sut.InstallPackage(package, false, false);

                var infos = ttl.MessagesFor(TraceLevel.Info);
                infos.Should().BeEquivalentTo(
                    $"Attempting to resolve dependency '{dependency.Id}'.", 
                    $"Installing '{dependency.Id} {dependency.Version}'.",
                    $"Successfully installed '{dependency.Id} {dependency.Version}'.",
                    $"Installing '{package.Id} {package.Version}'.",
                    $"Successfully installed '{package.Id} {package.Version}'.");
            }

            package.Received().GetCompatiblePackageDependencies(targetFramework);
            sut.SourceRepository.Received().GetPackages();
        }

        [Test]
        public void Ignore_AlreadyInstalled_Packages()
        {
            var context = new ContextFor<NuPlugPackageManager>();

            var packageLookup = Substitute.For<IPackageLookup>();
            var packageRegistry = new NuPlugPackageRegistry();
            var localRepo = new NuPlugPackageRepository(packageLookup, packageRegistry);

            var sut = context.BuildSut();
            var foo = "foo".CreatePackage("0.1.0");
            var bar = "bar".CreatePackage("0.1.1");
            var deps = new[] { new PackageDependencySet(sut.TargetFramework, new[] { new PackageDependency(bar.Id) }) };
            foo.DependencySets.Returns(deps);

            packageRegistry.Add(bar);

            var remotePackages = new[] { foo };
            sut.SourceRepository.GetPackages().Returns(remotePackages.AsQueryable());
            sut.SetLocalRepository(localRepo);

            sut.Logger = new TraceLogger();
            using (var tl = new TestTraceListener())
            {
                sut.InstallPackage(foo, false, true);

                var infos = tl.MessagesFor(TraceLevel.Info);
                infos.Should().BeEquivalentTo(
                    $"Attempting to resolve dependency '{bar.Id}'.",
                    //$"{bar.Id} already installed.",
                    $"Installing '{foo.Id} {foo.Version}'.",
                    $"Successfully installed '{foo.Id} {foo.Version}'."
                    );
            }
        }
    }
}