using NEdifis.Attributes;
using NSubstitute;
using NuGet;

namespace NuPlug
{
    [ExcludeFromConventions("testing helper")]
    internal static class TestingHelper
    {
        public static IPackage CreatePackage(this string id, string version)
        {
            var package = Substitute.For<IPackage>();
            package.Id.Returns(id);
            package.Version.Returns(SemanticVersion.Parse(version));
            return package;
        }
    }
}