using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NEdifis.Diagnostics;
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

        [Test]
        public void Not_break_on_invalid_packages_config_resources()
        {
            var sut = new NuPlugPackageRegistry();

            using (var tl = new TestTraceListener())
            {
                sut.ReadPackagesConfig(() => null);
                tl.MessagesFor(TraceLevel.Warning).Should().BeEmpty("silently ignore null streams");

                using (var stream = new MemoryStream())
                    sut.ReadPackagesConfig(() => stream);
                tl.MessagesFor(TraceLevel.Warning).Single().Should().StartWith("Could not read embedded resource 'packages.config': System.Xml.XmlException");
            }
        }
    }
}