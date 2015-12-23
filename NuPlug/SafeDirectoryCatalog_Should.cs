using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NEdifis.Attributes;
using NEdifis.Diagnostics;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(SafeDirectoryCatalog))]
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

        [Test]
        [Issue("#7", Title = "Redirect & Performance: Should support assembly filter, https://github.com/awesome-inc/NuPlug/issues/7")]
        public void Use_file_filter()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var dir = assembly.GetDirectory();

            // without file filter we load n assemblies
            using (var sut = new SafeDirectoryCatalog(dir))
                sut.Catalog.Catalogs.Should().NotBeNullOrEmpty();

            // exactly our assembly
            var assemblyFileName = assembly.GetLocation();
            using (var sut = new SafeDirectoryCatalog(dir, fileFilter: fileName => fileName.Equals(assemblyFileName, StringComparison.InvariantCultureIgnoreCase)))
            {
                var catalog = sut.Catalog.Catalogs.OfType<SafeAssemblyCatalog>().Single();
                catalog.Assembly.FullName.Should().Be(assembly.FullName);
            }

            // no assembly
            using (var sut = new SafeDirectoryCatalog(dir, fileFilter: fileName => false))
                sut.Catalog.Catalogs.Should().BeEmpty("file filter cannot match.");
        }

        [Test]
        public void Catch_assembly_load_exceptions()
        {
            AssertCatch(new ReflectionTypeLoadException(new Type[] { }, new Exception[] { }));
            AssertCatch(new BadImageFormatException("message"));

            0.Invoking(x => AssertCatch(new ArgumentException("test")))
                .ShouldThrow<ArgumentException>().WithMessage("test");
        }

        private static void AssertCatch(Exception ex)
        {
            var throwingTypeFilter = (Func<Type, bool>)(type =>
            {
                throw ex;
#pragma warning disable 162
                return true;
#pragma warning restore 162

            });

            var assembly = Assembly.GetExecutingAssembly();

            using (var ttl = new TestTraceListener { ActiveTraceLevel = TraceLevel.Verbose })
            {
                using (new SafeDirectoryCatalog(assembly.GetDirectory(), throwingTypeFilter))
                    ttl.MessagesFor(TraceLevel.Verbose).Should().OnlyContain(msg => msg.StartsWith("Could not load '"));

                ttl.MessagesFor(TraceLevel.Info).Should().BeEmpty("should only log at Verbose level");
            }
        }
    }
}
