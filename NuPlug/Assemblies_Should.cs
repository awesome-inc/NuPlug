using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (Assemblies))]
    // ReSharper disable InconsistentNaming
    internal class Assemblies_Should
    {
        [Test]
        public void Get_Assembly_Location()
        {
            var assembly = typeof (Assemblies_Should).Assembly;
            var actual = assembly.GetLocation();
            actual.Should().NotBeNullOrWhiteSpace();
            actual.Should().StartWith(assembly.GetDirectory());
            actual.Should().EndWith("NuPlug.DLL");
        }
    }
}