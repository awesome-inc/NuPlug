using System.Linq;
using System.Reflection;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (SafeDirectoryCatalog))]
    // ReSharper disable once InconsistentNaming
    internal class SafeDirectoryCatalog_Should
    {
        [Test]
        public void Use_type_filter()
        {
            var dir = Assembly.GetExecutingAssembly().GetDirectory();
            var expected = typeof(SafeDirectoryCatalog_Should);
            using (var sut = new SafeDirectoryCatalog(dir, type => type == expected))
            {
                var part = sut.Parts.Single();
                part.ToString().Should().Be(expected.FullName);
            }
        }
    }
}