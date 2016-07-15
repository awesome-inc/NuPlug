using System.Xml.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (NullPackage))]
    // ReSharper disable once InconsistentNaming
    internal class NullPackage_Should
    {
        [Test]
        public void Be_Creatable()
        {
            var version = SemanticVersion.Parse("0.1.0");
            var sut = new NullPackage("foo", version);

            sut.Should().BeAssignableTo<IPackage>();
            sut.Should().BeAssignableTo<LocalPackage>();

            sut.Id.Should().Be("foo");
            sut.Version.Should().Be(version);
            sut.Listed.Should().BeTrue();

            sut.DependencySets.Should().BeEmpty();
            sut.AssemblyReferences.Should().BeEmpty();
            using (var stream = sut.GetStream())
            {
                var xDoc = XDocument.Load(stream);
                xDoc.Should().HaveRoot("package")
                    .Which.Should().HaveElement("metadata")
                    .Which.Should().HaveElement("id", "foo")
                    .And.HaveElement("version", version.ToString());
            }

            sut.ExtractContents(null, null);
            sut.GetFiles().Should().BeEmpty();
        }
    }
}