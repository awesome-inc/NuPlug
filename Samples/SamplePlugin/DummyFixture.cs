//#if false

using System.ComponentModel.Composition;
using NUnit.Framework;

namespace SamplePlugin
{
    [PartNotDiscoverable]
    [TestFixture(Description = "dummy fixture to test assembly load behavior (here: NUnit2 vs 3)")]
    internal class DummyFixture
    {
        [Test]
        public void DummyTest()
        {
            Assert.Pass("Yahoo!");
        }
    }
}
//#endif