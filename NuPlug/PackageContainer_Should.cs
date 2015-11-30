using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(PackageContainer<>))]
    // ReSharper disable InconsistentNaming
    internal class PackageContainer_Should
    {
        [Test]
        public void Compose_on_Update()
        {
            var sut = new PackageContainer<IDisposable>();

            var dummy = Substitute.For<IDisposable>();
            sut._batch.AddExportedValue(dummy);

            sut.Update();

            sut.Items.Single().Should().Be(dummy);
        }

        [Test]
        public void Raise_Updated_event()
        {
            var sut = new PackageContainer<IDisposable>();
            var raisedUpdated = false;
            sut.Updated += (s, e) => { raisedUpdated = true; };
            sut.Update();
            raisedUpdated.Should().BeTrue();
        }

        [Test]
        [TestCase(typeof(PackageContainer<string>), true, "is a public IDisposable implementation")]
        [TestCase(typeof(SafeAssemblyCatalog), false, "is a private IDisposable implementation")]
        [TestCase(typeof(string), false, "public but not an IDisposable")]
        [TestCase(typeof(IDisposable), false, "public but not an implementation (interface only)")]
        [TestCase(typeof(ComposablePartCatalog), false, "public IDisposable but not an implementation (abstract)")]
        public void Consider_only_public_implementations_of_TItems_by_default(Type type, bool expected, string because)
        {
            var sut = new PackageContainer<IDisposable>();
            sut.TypeFilter(type).Should().Be(expected, because);
        }
    }
}