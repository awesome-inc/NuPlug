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
#if DEBUG
                var sw = Stopwatch.StartNew();
#endif
                var container = Configure(CreatePackageContainer());

#if DEBUG
                var totalTime = sw.Elapsed;
                foreach (var profiler in Profiler.AllProfilers.Values)
                    profiler.Print(totalTime);
#endif

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
                var repo = PackageRepositories.For(
                @"..\..\..\feed" // see 'Sample.targets'
                , "https://nuget.org/api/v2/");

            var packageManager = new NuPlugPackageManager(repo, "plugins") { Logger = new TraceLogger() };

            packageManager.SkipPackages();

            var version =
#if NCRUNCH
                Assembly.GetEntryAssembly().GetName().Version.ToString();
#else
                GitVersionInformation.NuGetVersion;
#endif
            var packagesConfig = new XDocument(
                new XElement("packages",
                    new XElement("package", new XAttribute("id", "NuPlug.SamplePlugin"), new XAttribute("version", version))
                    //, new XElement("package", new XAttribute("id", "NuPlug.RestPlugin"), new XAttribute("version", version))
                    ));

            Trace.TraceInformation($"Installing packages ({PrettySource(packageManager.SourceRepository)} --> {PrettySource(packageManager.LocalRepository)})...");
            packageManager.InstallPackages(packagesConfig, false, true);

            Trace.TraceInformation("Removing duplicates...");
            packageManager.RemoveDuplicates();

            return new NuGetPackageContainer<IModule>(packageManager);
        }

        private static string FormatException(Exception ex)
        {
            var rtlEx = ex as ReflectionTypeLoadException;
            var e = rtlEx?.LoaderExceptions.FirstOrDefault();
            return e?.ToString() ?? ex.ToString();
        }

        private static string PrettySource(IPackageRepository repository)
        {
            var aggregateRepository = repository as AggregateRepository;
            if (aggregateRepository != null)
                return string.Join(", ", aggregateRepository.Repositories.Select(PrettySource));

            return $"'{repository.Source}'";
        }
    }
}
