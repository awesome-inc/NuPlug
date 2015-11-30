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
            var aggRepo = (AggregateRepository)PackageRepositories.Create("a", "b");
            var repos = aggRepo.Repositories.ToArray();
            repos.Should().HaveCount(2);
            repos[0].Should().BeOfType<LazyLocalPackageRepository>();
            repos[1].Should().BeOfType<LazyLocalPackageRepository>();

            var repo = PackageRepositories.Create("a");
            repo.Should().BeOfType<LazyLocalPackageRepository>();

            repo = PackageRepositories.Create("http://a");
            repo.Should().BeOfType<DataServicePackageRepository>();
        }
    }
}