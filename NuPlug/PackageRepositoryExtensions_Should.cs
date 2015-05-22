using System.Linq;
using NEdifis.Attributes;
using NSubstitute;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof(PackageRepositoryExtensions))]
    // ReSharper disable InconsistentNaming
    class PackageRepositoryExtensions_Should
    {
        [Test]
        public void Remove_older_duplicates()
        {
            var packageRepository = Substitute.For<IPackageRepository>();

            var packages = new[]
            {
                CreatePackage("foo", "0.9.0")
                , CreatePackage("foo", "0.9.1")
                , CreatePackage("foo", "1.0.0")
            }.AsQueryable();

            packageRepository.GetPackages().Returns(packages);

            packageRepository.RemoveDuplicates();

            packageRepository.Received(2)
                .RemovePackage(Arg.Is<IPackage>(p => p.Version.Version.Major < 1));
        }

        static IPackage CreatePackage(string id, string version)
        {
            var package = Substitute.For<IPackage>();
            package.Id.Returns(id);
            package.Version.Returns(SemanticVersion.Parse(version));
            return package;
        }
    }
}