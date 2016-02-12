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
                Do(x => { throw new InvalidOperationException("test"); });

            packageManager.Invoking(x => x.InstallPackages(xdoc))
                .ShouldThrow<AggregateException>()
                .WithMessage("Error while installing packages")
                .WithInnerExceptionExactly<InvalidOperationException>()
                .WithInnerMessage("test");
        }

        [Test]
        public void Safely_install_pre_release_packages()
        {
            var packageManager = Substitute.For<IPackageManager>();

            const string packageId = "Caliburn.Micro.TestingHelpers";
            var version = new SemanticVersion("0.1.1-beta0001");

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
        public void Read_Settings_From_Xml()
        {
            const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
    <package version=""0.1.1-beta0001"" id=""Caliburn.Micro.TestingHelpers"" targetFramework=""net452"" />
</packages>";
            var xdoc = XDocument.Parse(xml);

            var packageManager = Substitute.For<IPackageManager>();

            const string packageId = "Caliburn.Micro.TestingHelpers";
            var version = new SemanticVersion("0.1.1-beta0001");

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
        [TestCase("0.1.1-beta0001", 0, 1, 1, "beta0001")]
        public void SemanticVersionParserTests(string ver, int major, int minor, int build, string special)
        {
            var version = SemanticVersion.Parse(ver);
            version.Version.Major.Should().Be(major);
            version.Version.Minor.Should().Be(minor);
            version.Version.Build.Should().Be(build);
            version.SpecialVersion.Should().Be(special);
        }

        [Test]
        [TestCase("0.1.1-unstable.123", 0, 1, 1, "unstable.123")]
        public void SemanticVersionParserTests_NotWorkingdirectly(string ver, int major, int minor, int build, string special)
        {
            // regular does not work
            new Action(() => SemanticVersion.Parse(ver)).ShouldThrow<ArgumentException>();

            // but in parts (reverse) ir works
            var v = new SemanticVersion(new Version(major, minor, build), special);
            v.ToString().Should().Be(string.Format($"{major}.{minor}.{build}-{special}"));

            // my cool code works
            var v2 = PackageManagerExtensions.SemanticVersionParseCustom(ver);

            v2.ShouldBeEquivalentTo(v);
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