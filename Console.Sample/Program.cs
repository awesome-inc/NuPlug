using System.Collections.Generic;
using System.Xml.Linq;
using Autofac;
using NuGet;
using NuPlug;

namespace ConsoleSample
{
    static class Program
    {
        static void Main(string[] args)
        {
            // ReSharper disable once ConvertToConstant.Local
            //var packageSource = "https://packages.nuget.org/api/v2";
            var packageSource = "http://localhost:8888/nuget/feed"; // cf.: https://github.com/mkoertgen/hello.nuget.server
            var feed = PackageRepositoryFactory.Default.CreateRepository(packageSource);

            var packageManager = new PackageManager(feed, "plugins");

            var packagesConfig = new XDocument(
                new XElement("packages",
                    new XElement("package", new XAttribute("id", "NuPlug.SamplePlugin"), new XAttribute("version", "1.0.0.0"))
                ));

            packageManager.InstallPackages(packagesConfig);

            var modulePlugins = new NugetPackageContainer<Module>(packageManager);

            Configure(modulePlugins.Items);
        }

        private static void Configure(IEnumerable<Module> modules)
        {
            var builder = new ContainerBuilder();

            foreach (var module in modules)
                builder.RegisterModule(module);

            var container = builder.Build();
        }
    }
}
