using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Autofac;
using Autofac.Core;
using NuGet;
using NuPlug;
using PluginContracts;

namespace ConsoleSample
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var container = Configure(CreatePackageContainer());

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unhandled exception: {0}", FormatException(ex));
                Environment.Exit(1);
            }
        }

        private static IContainer Configure(IPackageContainer<IModule> packageContainer)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<PluginHost>().As<IPluginHost>().SingleInstance();

            Trace.TraceInformation("Register plugins...");

            if (!packageContainer.Items.Any())
                packageContainer.Update();

            foreach (var module in packageContainer.Items)
                builder.RegisterModule(module);

            return builder.Build();
        }

        private static IPackageContainer<IModule> CreatePackageContainer()
        {
            // ReSharper disable once ConvertToConstant.Local
            var repo = PackageRepositories.Create(
                @"..\..\..\feed" // see 'Plugin.targets'
                // , "http://localhost:8888/nuget/feed" // cf.: https://github.com/mkoertgen/hello.nuget.server
                , "https://packages.nuget.org/api/v2");

            var packageManager = new PackageManager(repo, "plugins") {Logger = new TraceLogger()};

            //packageManager.PackageInstalling += (sender, args) => Trace.TraceInformation("Adding package '{0} {1}' to '{2}'", args.Package.Id, args.Package.Version, args.InstallPath);
            //packageManager.PackageInstalled += (sender, args) => Trace.TraceInformation("Successfully installed '{0} {1}'", args.Package.Id, args.Package.Version);
            //packageManager.PackageUninstalling += (sender, args) => Trace.TraceInformation("Removing package '{0} {1}' to '{2}'", args.Package.Id, args.Package.Version, args.InstallPath);
            //packageManager.PackageUninstalled += (sender, args) => Trace.TraceInformation("Successfully uninstalled '{0} {1}'", args.Package.Id, args.Package.Version);

            var version = Assembly.GetEntryAssembly().GetName().Version.ToString();
#if !NCRUNCH
            version = GitVersionInformation.NuGetVersion;
#endif
            var packagesConfig = new XDocument(
                new XElement("packages",
                    new XElement("package", new XAttribute("id", "NuPlug.SamplePlugin"), new XAttribute("version", version))
                    , new XElement("package", new XAttribute("id", "NuPlug.RestPlugin"), new XAttribute("version", version))
                    ));

            Trace.TraceInformation("Installing packages...");
            packageManager.InstallPackages(packagesConfig, false);

            Trace.TraceInformation("Removing duplicates...");
            packageManager.RemoveDuplicates();

            return new NugetPackageContainer<IModule>(packageManager);
        }

        private static string FormatException(Exception ex)
        {
            var rtlEx = ex as ReflectionTypeLoadException;
            var e = rtlEx?.LoaderExceptions.FirstOrDefault();
            if (e != null) return e.ToString();

            return ex.ToString();
        }
    }
}
