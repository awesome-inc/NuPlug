using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NEdifis.Attributes;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (SafeAssemblyCatalog))]
    // ReSharper disable once InconsistentNaming
    internal class SafeAssemblyCatalog_Should
    {
        [Test]
        public void Use_type_filter()
        {
            var expected = typeof (SafeAssemblyCatalog_Should);
            using (var sut = new SafeAssemblyCatalog(Assembly.GetExecutingAssembly(), type => type == expected))
            {

                // "is a" ReflectionComposablePartDefinition(), so we use "ToString()" to check
                // cf.: https://mef.codeplex.com/SourceControl/latest#redist/src/ComponentModel/System/ComponentModel/Composition/ReflectionModel/ReflectionComposablePartDefinition.cs
                var part = sut.Parts.Single();
                part.ToString().Should().Be(expected.FullName);


                var importDefinition = new ImportDefinition(e => false, "contract", ImportCardinality.ExactlyOne, false,
                    false);
                sut.GetExports(importDefinition).Should().BeEmpty();
            }
        }
    }
}