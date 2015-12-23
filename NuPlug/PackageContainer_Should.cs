using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Registration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using NEdifis.Attributes;
using NSubstitute;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (PackageContainer<>))]
    // ReSharper disable InconsistentNaming
    internal class PackageContainer_Should
    {
        [Test]
        public void Compose_on_Update()
        {
            using (var sut = new PackageContainer<IDisposable>())
            {

                var dummy = Substitute.For<IDisposable>();
                sut.Batch.AddExportedValue(dummy);

                sut.Update();
                sut.Items.Single().Should().Be(dummy);
            }
        }

        [Test]
        public void Raise_Updated_event()
        {
            using (var sut = new PackageContainer<IDisposable>())
            {
                var raisedUpdated = false;
                sut.Updated += (s, e) => { raisedUpdated = true; };
                sut.Update();
                raisedUpdated.Should().BeTrue();
            }
        }

        [Test]
        [TestCase(typeof (PackageContainer<string>), true, "is a public IDisposable implementation")]
        [TestCase(typeof (SafeAssemblyCatalog), false, "is a private IDisposable implementation")]
        [TestCase(typeof (string), false, "public but not an IDisposable")]
        [TestCase(typeof (IDisposable), false, "public but not an implementation (interface only)")]
        [TestCase(typeof (ComposablePartCatalog), false, "public IDisposable but not an implementation (abstract)")]
        public void Consider_only_public_implementations_of_TItems_by_default(Type type, bool expected, string because)
        {
            using (var sut = new PackageContainer<IDisposable>())
                sut.TypeFilter(type).Should().Be(expected, because);
        }

        [Test]
        public void Support_customized_type_filters()
        {
            var expected = typeof (int);
            using (var sut = new PackageContainer<string> {TypeFilter = type => type == expected})
            {
                sut.TypeFilter(expected).Should().BeTrue();
                sut.TypeFilter(typeof (string)).Should().BeFalse();

                sut.Invoking(x => x.TypeFilter = null).ShouldThrow<ArgumentNullException>("null not allowed");
            }
        }

        [Test]
        public void Support_custom_reflection_context_for_using_MEF_conventions()
        {
            using (var sut = new PackageContainer<IDisposable>())
            {
                sut.Conventions.Should().BeNull();

                var conventions = new RegistrationBuilder();
                conventions.ForTypesDerivedFrom<IDisposable>()
                    .ExportInterfaces();

                sut.Conventions = conventions;
                sut.Conventions.Should().Be(conventions);
            }
        }

        [Test]
        public void Add_and_remove_directories()
        {
            var dir = GetRandomDirectory();
            Directory.CreateDirectory(dir);

            try
            {
                var dirs = new List<string> { dir };

                var assemblyResolver = Substitute.For<IResolveAssembly>();
                assemblyResolver.Directories.Returns(dirs);


                using (var sut = new PackageContainer<IDisposable>(assemblyResolver))
                {
                    sut.Update();
                    // ReSharper disable once NotAccessedVariable
                    var dummy = assemblyResolver.Received(2).Directories;

                    sut.Update();
                    // ReSharper disable once RedundantAssignment
                    dummy = assemblyResolver.Received(4).Directories;

                    var otherDir = GetRandomDirectory();
                    sut.AddDirectory(otherDir);
                    dirs.Should().BeEquivalentTo(dir, otherDir);

                    sut.AddDirectory(dir);
                    dirs.Should().BeEquivalentTo(dir, otherDir);

                    sut.RemoveDirectory(dir);
                    dirs.Should().BeEquivalentTo(otherDir);

                    sut.RemoveDirectory(otherDir);
                    dirs.Should().BeEmpty();
                }
            }
            finally { Directory.Delete(dir); }
        }

        [Test]
        [Issue("#7", Title = "Redirect & Performance: Should support assembly filter, https://github.com/awesome-inc/NuPlug/issues/7")]
        public void Support_customized_file_filters()
        {
            var regex = new Regex("Plugin.dll$");
            using (var sut = new PackageContainer<string> { FileFilter = regex.IsMatch })
            {
                sut.FileFilter(@"c:\some\path\packages\RestPlugin.1.2.3\lib\net45\RestPlugin.dll").Should().BeTrue();
                sut.FileFilter("Newtonsoft.Json.dll").Should().BeFalse();
                sut.Invoking(x => x.TypeFilter = null).ShouldThrow<ArgumentNullException>("null not allowed");
            }
        }


        private static string GetRandomDirectory()
        {
            var dir = Path.GetRandomFileName();
            File.Delete(dir);
            return dir;
        }
    }
}