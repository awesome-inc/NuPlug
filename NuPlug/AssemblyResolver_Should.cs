using System;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (AssemblyResolver))]
    // ReSharper disable InconsistentNaming
    internal class AssemblyResolver_Should
    {
        [Test]
        public void Resolve_assemblies()
        {
            using (var sut = new AssemblyResolver())
            {
                var expected = typeof (AssemblyResolver_Should).Assembly;
                var assembly = sut.ResolveAssembly(this, new ResolveEventArgs(expected.GetName().Name));
                
                //sut.Directories.Add();
                //assembly.Should()..BeNull();

                
                //actual.FullName.Should().Be(expected.FullName);
            }
        }
    }
}