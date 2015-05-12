using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(PackageContainer<>))]
    // ReSharper disable InconsistentNaming
    class MefPackageContainer_Should
    {
        [Test]
        public void Compose_on_Update()
        {
            var sut = new PackageContainer<IDisposable>();

            var dummy = Substitute.For<IDisposable>();
            sut.Batch.AddExportedValue(dummy);

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
    }
}