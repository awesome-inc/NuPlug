using System;
using FluentAssertions;
using NUnit.Framework;
using NEdifis.Attributes;

namespace NuPlug
{
    [TestFixtureFor(typeof(VersionHelper))]
    // ReSharper disable InconsistentNaming
    class VersionHelper_Should
    {
        [Test]
        public void Test_GetTargetFramework()
        {
            var actual = VersionHelper.GetTargetFramework();
            actual.FullName.Should().Be(".NETFramework,Version=v4.5");
        }

        [Test]
        [TestCase("4.5.2", "net20,net30,net40,net45,net46", "net45")]
        [TestCase("4.0", "net35,net40-client,net45", "net40-client")]
        [TestCase("3.5", "net20,net35,net40", "net35")]
        public void Test_BestMatch(string version, string folders, string expected)
        {
            new Version(version).BestMatch(folders.Split(',')).Should().Be(expected);
        }
    }
}