using System;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(PackageManagerExtensions))]
    // ReSharper disable InconsistentNaming
    internal class PackageManagerExtensions_Should
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
                Do(x => {throw new InvalidOperationException("test");});

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

            packageManager.Received(2).UninstallPackage(Arg.Is<IPackage>(p => p.Version.Version.Major < 1), false, false);
        }
    }
}