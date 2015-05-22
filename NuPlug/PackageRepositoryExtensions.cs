using System.Linq;
using NuGet;

namespace NuPlug
{
#if DEBUG
    [NEdifis.Attributes.TestedBy(typeof(PackageRepositoryExtensions_Should))]
#endif
    public static class PackageRepositoryExtensions
    {
        public static void RemoveDuplicates(this IPackageRepository packageRepository)
        {
            var duplicates = packageRepository.GetPackages()
                .GroupBy(p => p.Id)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.OrderByDescending(p => p.Version).Skip(1))
                .ToList();

            duplicates.ForEach(packageRepository.RemovePackage);
        }
    }
}