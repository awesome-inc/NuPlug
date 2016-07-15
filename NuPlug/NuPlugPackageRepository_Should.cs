using System.Linq;
using FluentAssertions;
using NEdifis;
using NEdifis.Attributes;
using NSubstitute;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (NuPlugPackageRepository))]
    // ReSharper disable once InconsistentNaming
    internal class NuPlugPackageRepository_Should
    {
        [Test]
        public void Delegate_to_decorated_repo()
        {
            var ctx = new ContextFor<NuPlugPackageRepository>();
            var sut = ctx.BuildSut();

            sut.Should().BeAssignableTo<IPackageLookup>();

            var lookup = ctx.For<IPackageLookup>();

            sut.PackageSaveMode.Should().Be(lookup.PackageSaveMode);
            // ReSharper disable once RedundantAssignment
            var mode = lookup.Received().PackageSaveMode;
            mode = PackageSaveModes.Nupkg;
            sut.PackageSaveMode = mode;
            lookup.Received().PackageSaveMode = mode;

            lookup.SupportsPrereleasePackages.Returns(true);
            sut.SupportsPrereleasePackages.Should().BeTrue();
            // ReSharper disable once UnusedVariable
            var b = lookup.Received().SupportsPrereleasePackages;

            lookup.Source.Returns("source");
            sut.Source.Should().Be(lookup.Source);
            // ReSharper disable once UnusedVariable
            var s = lookup.Received().Source;

            sut.GetPackages().Should().BeEmpty();
            lookup.Received().GetPackages();

            var package = "foo".CreatePackage("0.1.0");
            sut.AddPackage(package);
            lookup.Received().AddPackage(package);

            sut.RemovePackage(package);
            lookup.Received().RemovePackage(package);
        }

        [Test]
        public void Lookup_packages()
        {
            var ctx = new ContextFor<NuPlugPackageRepository>();
            var sut = ctx.BuildSut();

            var lookup = ctx.For<IPackageLookup>();
            var registry = ctx.For<IPackageRegistry>();

            var package = "foo".CreatePackage("0.1.0");
            sut.Exists(package).Should().BeFalse();
            Received.InOrder(() =>
            {
                registry.Exists(package.Id, package.Version);
                lookup.Exists(package.Id, package.Version);
            });

            registry.FindPackage(package.Id, package.Version).Returns(package);
            sut.FindPackage(package.Id, package.Version).Should().Be(package);

            registry.FindPackage(package.Id, package.Version).Returns((IPackage)null);
            lookup.FindPackage(package.Id, package.Version).Returns(package);
            sut.FindPackage(package.Id, package.Version).Should().Be(package);

            lookup.FindPackage(package.Id, package.Version).Returns((IPackage)null);
            sut.FindPackage(package.Id, package.Version).Should().BeNull();

            sut.FindPackagesById(package.Id).Should().BeEmpty();

            var packages = new [] {package};
            registry.FindPackagesById(package.Id).Returns(packages);
            sut.FindPackagesById(package.Id).ShouldAllBeEquivalentTo(packages);

            var empty = Enumerable.Empty<IPackage>().ToList();
            registry.FindPackagesById(package.Id).Returns(empty);
            lookup.FindPackagesById(package.Id).Returns(packages);
            sut.FindPackagesById(package.Id).ShouldAllBeEquivalentTo(packages);

            lookup.FindPackagesById(package.Id).Returns(empty);
            sut.FindPackagesById(package.Id).Should().BeEmpty();
        }

        [Test]
        public void Ignore_AlreadyInstalled_Packages()
        {
            var ctx = new ContextFor<NuPlugPackageRepository>();
            var sut = ctx.BuildSut();

            var package = "foo".CreatePackage("0.1.0");
            sut.Exists(package.Id, package.Version).Should().BeFalse();

            ctx.For<IPackageRegistry>().Exists(package.Id, Arg.Any<SemanticVersion>()).Returns(true);
            sut.Exists(package.Id, package.Version).Should().BeTrue();
        }
    }
}