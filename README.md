
# NuPlug

Plugin management powered by [NuGet](https://github.com/nuget/home) and [MEF](https://msdn.microsoft.com/en-us/library/dd460648%28v=vs.110%29.aspx).

[![Join the chat at https://gitter.im/awesome-inc/NuPlug](https://badges.gitter.im/awesome-inc/NuPlug.svg)](https://gitter.im/awesome-inc/NuPlug?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Build status](https://ci.appveyor.com/api/projects/status/wl4qp6pr6067r7dl?svg=true)](https://ci.appveyor.com/project/awesome-inc-build/nuplug)

[![Nuget](https://img.shields.io/nuget/dt/nuplug.svg)](http://nuget.org/packages/nuplug)
[![Nuget](https://img.shields.io/nuget/v/nuplug.svg)](http://nuget.org/packages/nuplug)

[![Issue Stats](http://issuestats.com/github/awesome-inc/NuPlug/badge/issue)](http://issuestats.com/github/awesome-inc/NuPlug) 
[![Issue Stats](http://issuestats.com/github/awesome-inc/NuPlug/badge/pr)](http://issuestats.com/github/awesome-inc/NuPlug)
[![Coverage Status](https://coveralls.io/repos/awesome-inc/NuPlug/badge.svg?branch=develop&service=github)](https://coveralls.io/github/awesome-inc/NuPlug?branch=develop)

## What is it?

The idea of using NuGet for plugin management is not new, e.g. Visual Studio manages its extensions with NuGet as does JetBrains ReSharper. As we can find, most ideas date back to about 2011 when Matt Hamilton announced that [Comicster uses NuGet for plugins](http://matthamilton.net/nuget-for-plug-ins).

## Quick start

You should have a plugin and an application consuming the plugin.

### Application

First, install [NuPlug](https://github.com/awesome-inc/NuPlug) to your application

	Install-Package NuPlug

Then in the startup part, create a package manager. Here is an example snippet from the `ConsoleSample` application:

	var packageSource = "https://mynugetfeed/"; // UNC share, folder, ... 
	var feed = PackageRepositoryFactory.Default.CreateRepository(packageSource);
	var packageManager = new NuPlugPackageManager(feed, "plugins") 
      { 
         Logger = new TraceLogger(),
         TargetFramework = VersionHelper.GetTargetFramework()
      };

This will download NuGet packages from the specified package source to the output directory `plugins`.

Next, you need to specify which plugin packages to load. The most common way is to use a `packages.config` Xml file, e.g. 

	var packagesConfig = new XDocument(
		new XElement("packages",
			new XElement("package", new XAttribute("id", "NuPlug.SamplePlugin"), new XAttribute("version", version))
		));

As an alternative, you can use xml files or string for the configuration.

    const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
    <packages>
        <package version=""0.1.1-beta0001"" id=""Caliburn.Micro.TestingHelpers"" targetFramework=""net452"" />
    </packages>";
    var xdoc = XDocument.Parse(xml);


Then install your plugin packages by

	packageManager.InstallPackages(packagesConfig);

	// When plugins update, be sure to remove previous versions 
	packageManager.RemoveDuplicates();

Finally, load the installed packages using `NuGetPackageContainer<T>` typed to your plugin interface. The console sample uses [AutoFac modules](http://docs.autofac.org/en/latest/configuration/modules.html): 

	var modulePlugins = new NuGetPackageContainer<IModule>(packageManager);
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

#### Controlling type discovery from plugins

You can filter the types for MEF to discover by using the `TypeFilter` property of `PackageContainer<TItem>`. By default, the package container only discovers public implementations of `TItem`, i.e.

	TypeFilter = type =>
		type.IsPublic && type.IsClass && !type.IsAbstract && typeof(TItem).IsAssignableFrom(type);

This assumes that MEF does not need to resolve or compose any dependencies to instantiate the requested plugins.
Note that in the provided examples we use [AutoFac](http://autofac.org/) for dependency injection, not MEF.

#### Controlling assembly discovery from NuGet packages

As noted in [Issue #7](https://github.com/awesome-inc/NuPlug/issues/7) the MEF part may load an awfully large number of assemblies, especially when considering the full dependency tree. As this not only may cause large startup times (preventing just-in-time code loading) but may also bypass binding redirects of the main application.

Since v0.4 we added optional support for filtering the assemblies to be scanned by MEF

    var regex = new Regex("Plugin.dll$");
    var packageContainer = new PackageContainer<string> { FileFilter = regex.IsMatch });

Using the example above, the package container will only scan files with names matching the specified regular expression, in this case files ending with `Plugin.dll`.

#### Register plugin dependencies (MEF)

In case your plugins need dependencies, you can add these to the package container's [CompositionBatch](https://msdn.microsoft.com/en-us/library/system.componentmodel.composition.hosting.compositionbatch(v=vs.110).aspx). Here is an example

    [Export(typeof(IPlugin)
    public class MyPlugin : IPlugin
    {
        public MyClass(IPluginDependency dependency) { ... }
    }

Then setup the package container like this

    var packageContainer = new PackageContainer<IPlugin>();
    // add service provider to satisfy plugin constructors
	packageContainer.AddExportedValue(_serviceProvider);
    ...
    if (!packageContainer.Items.Any())
    	packageContainer.Update();

#### Using MEF conventions

You can even use MEF conventions by setting the `Conventions` property like this

    var conventions = new RegistrationBuilder();
    conventions.ForTypesDerivedFrom<IDisposable>()
        .ExportInterfaces();

    packageContainer.Conventions = conventions;
    ...
    if (!packageContainer.Items.Any())
    	packageContainer.Update();

Note that you use conventions only to select exports but not to *hide* types like with [PartNonDiscoverableAttribute](https://msdn.microsoft.com/en-us/library/system.componentmodel.composition.partnotdiscoverableattribute(v=vs.110).aspx). This is why we added the 
`TypeFilter` property.

## Selecting the target framework

There is good chance that your plugins themselves have runtime dependencies. This is called transitive dependencies and resolving these dependencies is what package managers like NuGet are really made for.

However, with more and more packages becoming cross-platform, you should only need to install the dependencies 
needed for the runtime target framework of your application. 

This is especially true, when you are hosting a NuGet feed for your plugins yourself.
As of the current [NuGet.Core.2.10.1](https://www.nuget.org/packages/NuGet.Core/2.10.1), the PackageManager does [not consider the target framework](https://github.com/NuGet/NuGet2/blob/2.10.1/src/Core/PackageManager.cs#L128) specified in the `packages.config`.

We think that this will be addressed soon by the Nuget team. Meanwhile you should use `NuPlugPackageManager` like in the example specified above, i.e.
  
	var packageManager = new NuPlugPackageManager(feed, "plugins") 
      { 
         Logger = new TraceLogger(),
         TargetFramework = VersionHelper.GetTargetFramework()
      }; 

## Where to go from here?

Some hints getting up to speed in production with NuPlug

### 1. Automate packaging
To get up to speed you should automate as much of the manual tasks as possible and make it part of your build process. For instance, we do NuGet packaging using [OneClickBuild](https://github.com/awesome-inc/OneClickBuild)).

### 2. Speeding up development cycles for `DEBUG`
During hot development, short feedback cycles are king. Note that, decoupling your code using plugins is cool but is likely to increase your development cycles unless you automate building and publishing the plugins within the standard Visual Studio build. To move fast, we totally skip NuGet packaging during `DEBUG` and just load the plugin assemblies from a directory. For this to work, you need two things

1. Have an `AfterBuild` target to copy your modules output to this directory. For instance, we include a [Sample.targets](Samples\Sample.targets) containing a step to auto-copy the build plugin package to the local feed directory 

		<PropertyGroup>
		    <UseLocalPackages Condition="'$(Configuration)' == 'Debug' And $(RootNamespace.EndsWith('Plugin')) And '$(NCrunch)' != '1'">True</UseLocalPackages>
		  </PropertyGroup>
		
		  <Target Name="CopyLocalPackage" DependsOnTargets="Package" AfterTargets="Build" Condition="'$(UseLocalPackages)' == 'True' " >
		    <ItemGroup>
		      <Packages Include="$(ProjectDir)\*.nupkg"/>
		    </ItemGroup>
		    <Message Text="Copying Package '$(ProjectName)' to output ..." Importance="High" Condition="'@(Packages->Count())' &gt; 0"/>
		    <Copy SourceFiles="@(Packages)"
		          DestinationFolder="$(SolutionDir)Samples\feed\"
		          SkipUnchangedFiles="True"
		          Condition="'@(Packages->Count())' &gt; 0"/>
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

with `localPath ~= ..\..\..\feed\`. 
 