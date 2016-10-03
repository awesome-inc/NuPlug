using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NuPlug
{
    internal class SafeDirectoryCatalog : ComposablePartCatalog
    {
        internal readonly AggregateCatalog Catalog;
        private readonly DirectoryInfo _dirInfo;

        public string FullPath => _dirInfo.FullName;
        public override IQueryable<ComposablePartDefinition> Parts => Catalog.Parts;
        public IList<Assembly> Assemblies { get; } = new List<Assembly>();

        public SafeDirectoryCatalog(string directory, 
            Func<Type,bool> typeFilter = null, 
            ReflectionContext reflectionContext = null,
            Func<string, bool> fileFilter = null)
        {
            // cf.: http://stackoverflow.com/a/4475117/2592915
            _dirInfo = new DirectoryInfo(directory);
            Catalog = new AggregateCatalog();

            var safeFileFilter = fileFilter ?? (fileName => true);
            var files = _dirInfo.EnumerateFiles("*.dll", SearchOption.AllDirectories)
                .Where(file => safeFileFilter(file.FullName));
            foreach (var file in files)
            {
                try
                {
                    var catalog = new SafeAssemblyCatalog(file.FullName, typeFilter, reflectionContext);

                    //Force MEF to load the plugin and figure out if there are any exports.
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (catalog.Parts.ToList().Count <= 0)
                        continue;

                    Catalog.Catalogs.Add(catalog);
                    Assemblies.Add(catalog.Assembly);
                }
                catch (Exception ex) when (ex is ReflectionTypeLoadException || ex is BadImageFormatException)
                {
                    Trace.WriteLine($"Could not load '{file.FullName}': {ex}");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Catalog.Dispose();
        }
    }
}