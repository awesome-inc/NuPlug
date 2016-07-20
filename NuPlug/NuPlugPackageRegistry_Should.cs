using System;
using FluentAssertions;
using NEdifis.Attributes;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (NuPlugPackageRegistry))]
    // ReSharper disable once InconsistentNaming
    internal class NuPlugPackageRegistry_Should
    {
        [Test]
        public void Be_Creatable()
        {
            var sut = new NuPlugPackageRegistry();

            var package = "foo".CreatePackage("0.1.0");

            sut.Exists(package.Id).Should().BeFalse();

            sut.Add(package);
            sut.Add(package.Id, package.Version);

            sut.Exists(package.Id).Should().BeTrue();
            sut.Exists(package.Id, package.Version).Should().BeTrue();
            sut.Exists(package.Id, SemanticVersion.Parse("0.2.0")).Should().BeFalse();

            sut.Invoking(x => x.FindPackage(null, null)).ShouldThrow<ArgumentException>();
            sut.Invoking(x => x.FindPackage(package.Id, null)).ShouldThrow<ArgumentException>();
            sut.Invoking(x => x.FindPackage("\t\r\n", package.Version)).ShouldThrow<ArgumentException>();

            sut.FindPackage(package.Id, package.Version).ShouldBeEquivalentTo(package);
            sut.FindPackage(package.Id, SemanticVersion.Parse("0.2.0")).Should().BeNull();

            sut.FindPackagesById(package.Id).ShouldAllBeEquivalentTo(new[] {package});
            sut.FindPackagesById("bar").Should().BeEmpty();
        }
    }
}