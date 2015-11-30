using NUnit.Framework;

namespace SamplePlugin
{
    [TestFixture(Description = "dummy fixture to test assembly load behavior (here: NUnit2 vs 3)")]
    // ReSharper disable once InconsistentNaming
    internal class Dummy_Should
    {
        [Test]
        public void DummyTest()
        {
            Assert.Pass("Yahoo!");
        }
    }
}
