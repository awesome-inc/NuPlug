using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (NuPlugExtensions))]
    // ReSharper disable InconsistentNaming
    internal class NuPlugExtensions_Should
    {
        [Test]
        public void Safely_install_packages()
        {
            var packageManager = Substitute.For<IPackageManager>();

            const string packageId = "NUnit";
            var version = new SemanticVersion("2.6.4");

            var xdoc = new XDocument(new XElement("packages",
                new XElement("package",
                    new XAttribute("id", packageId),
                    new XAttribute("version", version.ToString()))));

            packageManager.InstallPackages(xdoc);
            packageManager.Received().InstallPackage(packageId, version, true, false);

            packageManager.When(x => x.InstallPackage(packageId, version, true, false)).
                Do(x => { throw new InvalidOperationException("test"); });

            packageManager.Invoking(x => x.InstallPackages(xdoc))
                .ShouldThrow<AggregateException>()
                .WithMessage("Error while installing packages")
                .WithInnerExceptionExactly<InvalidOperationException>()
                .WithInnerMessage("test");
        }

        [Test]
        public void Remove_older_duplicates()
        {
            var packageManager = Substitute.For<IPackageManager>();

            var packages = new[]
            {
                "foo".CreatePackage("0.9.0")
                , "foo".CreatePackage("0.9.1")
                , "foo".CreatePackage("1.0.0")
            };

            packageManager.LocalRepository.GetPackages().Returns(packages.AsQueryable());

            packageManager.RemoveDuplicates();

            packageManager.Received(2)
                .UninstallPackage(Arg.Is<IPackage>(p => p.Version.Version.Major < 1), false, false);
        }

        [Test, Issue("https://github.com/awesome-inc/NuPlug/issues/11")]
        public void Support_an_Api_to_ignore_packages()
        {
            var sut = Substitute.For<ISkipPackages>();

            sut.SkipPackages(typeof(NuPlugPackageManager_Should).Assembly, "packages.config");
            sut.DidNotReceiveWithAnyArgs().SkipPackages(null); // no "packages.config" embedded in this assembly


            var expected = new[]
            {
                new PackageName("foo", SemanticVersion.Parse("0.1.0"))
            };

            var actual = new List<IPackageName>();
            sut.When(x => x.SkipPackages(Arg.Any<IEnumerable<IPackageName>>()))
                .Do(x => actual.AddRange(x.Arg<IEnumerable<IPackageName>>()));
            var packagesConfig = new XDocument(new XElement("packages",
                expected.Select(p => new XElement("package", new XAttribute("id", p.Id), new XAttribute("version", p.Version.ToString())) )));

            sut.SkipPackages(packagesConfig);

            actual.ShouldAllBeEquivalentTo(expected);
        }
    }
}