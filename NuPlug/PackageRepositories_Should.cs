using System.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (PackageRepositories))]
    // ReSharper disable InconsistentNaming
    internal class PackageRepositories_Should
    {
        [Test]
        public void Create_repositories()
        {
            var aggRepo = (AggregateRepository)PackageRepositories.For("a", "b");
            var repos = aggRepo.Repositories.ToArray();
            repos.Should().HaveCount(2);
            repos[0].Should().BeOfType<LazyLocalPackageRepository>();
            repos[1].Should().BeOfType<LazyLocalPackageRepository>();

            var repo = PackageRepositories.For("a");
            repo.Should().BeOfType<LazyLocalPackageRepository>();

            repo = PackageRepositories.For("http://a");
            repo.Should().BeOfType<DataServicePackageRepository>();
        }
    }
}