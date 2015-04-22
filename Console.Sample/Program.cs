using System;
using NuGet;
using NuPlug;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            // cf.: https://github.com/mkoertgen/hello.nuget.server
            var packageSource = "http://localhost:8888/nuget/feed"; //"https://packages.nuget.org/api/v2";
            var feedRepo = PackageRepositoryFactory.Default.CreateRepository(packageSource);

            var packageManager = new PackageManager(feedRepo, "plugins");
            var plugins = new PlugIns(packageManager);

            plugins.Composed += (s, e) => Console.WriteLine("plugin action");

            packageManager.InstallPackage("NUnit");


            packageManager.UninstallPackage("NUnit");
        }
    }
}
