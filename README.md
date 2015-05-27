[![Build status](https://ci.appveyor.com/api/projects/status/247pcwhcvr4177at/branch/master?svg=true)](https://ci.appveyor.com/project/awesome-inc-build/nuplug) ![NuGet Version](https://img.shields.io/nuget/v/NuPlug.svg?style=flat-square) ![NuGet Version](https://img.shields.io/nuget/dt/NuPlug.svg?style=flat-square)

# NuPlug

Plugin management powered by [NuGet](https://github.com/nuget/home) and [MEF](https://msdn.microsoft.com/en-us/library/dd460648%28v=vs.110%29.aspx).

## What is it?

The idea of using NuGet for plugin management is not new, e.g. Visual Studio manages its extensions with NuGet as does JetBrains ReSharper. As we can find, most ideas date back to about 2011 when Matt Hamilton announced that [Comicster uses NuGet for plugins](http://matthamilton.net/nuget-for-plug-ins).

## Quick start

You should have a plugin and an application consuming the plugin.

### Application

First, install [NuPlug](https://github.com/awesome-inc/NuPlug) to your application

	Install-Package NuPlug

Then in the startup part, create a NuGet Package Manager. Here is an example snippet from the `ConsoleSample` application:

    var packageSource = "https://mynugetfeed/"; // UNC share, folder, ... 
    var feed = PackageRepositoryFactory.Default.CreateRepository(packageSource);
	var packageManager = new PackageManager(feed, "plugins");

This will download NuGet packages from the specified package source to the output directory `plugins`.

Next, you need to specify which plugin packages to load. The most common way is to use a `packages.config` Xml file, e.g. 

    var packagesConfig = new XDocument(
        new XElement("packages",
            new XElement("package", new XAttribute("id", "NuPlug.SamplePlugin"), new XAttribute("version", "0.1.5.0"))
        ));

Then install your plugin packages by

    packageManager.InstallPackages(packagesConfig);

	// When plugins update, be sure to remove previous versions 
    packageManager.RemoveDuplicates();

Finally, load the installed packages using `NuGetPackageContainer<T>` typed to your plugin interface. The console sample uses [AutoFac modules](http://docs.autofac.org/en/latest/configuration/modules.html): 

    var modulePlugins = new NugetPackageContainer<IModule>(packageManager);
    modulePlugins.Update();

    var builder = new ContainerBuilder();
    
	foreach (var module in modulePlugins.Items)
        builder.RegisterModule(module);

    var container = builder.Build();

### Plugin

For the plugin part, you need to [export](https://msdn.microsoft.com/en-us/library/dd460648(v=vs.110).aspx#imports_and_exports_with_attributes) your implementation as the specified plugin contract interface. The sample application specified `IModule` as contract, so for the `SamplePlugin`

	[Export(typeof(IModule))]
    public class MyPluginModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Trace.TraceInformation("Load: " + GetType().Name);
            base.Load(builder);
        }
    }

The [build a NuGet package](https://docs.nuget.org/create/creating-and-publishing-a-package) of your plugin project, push it to your feed and you are set.

## Where to go from here?

Some hints getting up to speed in production with NuPlug

### 1. Automate packaging
To get up to speed you should automate as much of the manual tasks as possible and make it part of your build process. For instance, we do NuGet packaging using [OneClickBuild](https://github.com/awesome-inc/OneClickBuild)).

### 2. Cutting corners in `DEBUG`
During hot development, short feedback cycles are king. Note that, decoupling your code using plugins is cool but is likely to increase your development cycles unless you automate building and publishing the plugins within the standard Visual Studio build. To move fast, we totally skip NuGet packaging during `DEBUG` and just load the plugin assemblies from a directory. For this to work, you need two things

1. Have an `AfterBuild` target to copy your modules output to this directory. For instance, we include a `Module.targets` containing

		<Target Name="CopyModuleLocal" AfterTargets="Build" Condition="'$(Configuration)'=='Debug'">
		  <ItemGroup>
		    <ModuleBinaries Include="$(OutDir)\**\*.*"/>
		  </ItemGroup>
		  <Message Text="Copying Module '$(ProjectName)' to output ..." Importance="High" />
		  <Copy SourceFiles="@(ModuleBinaries)" 
		    DestinationFolder="$(SolutionDir)Modules\_output\$(ProjectName)\%(RecursiveDir)"
		    SkipUnchangedFiles="True"/>
		</Target>
	
2. Have a factory for the `IPackageContainer<T>` deciding which implementation to use at runtime. For `DEBUG` just use the `PackageContainer<T>` base class like this:

		private static IPackageContainer<TItem> CreateDirectoryContainer(string localPath)
		{
		    var packageContainer = new PackageContainer<TItem>();
		    if (Directory.Exists(localPath))
		    {
		        foreach (var directory in Directory.GetDirectories(localPath))
		            packageContainer.AddDirectory(directory);
		    }
		    else
		    {
		        Trace.TraceWarning("Packages directory \"{0}\" does not exist", localPath);
		    }
		    return packageContainer;
		}

with `localPath ~= ..\..\..\Modules\_output\`. 
 