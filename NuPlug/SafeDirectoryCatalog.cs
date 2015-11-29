using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NuPlug
{
    internal class SafeDirectoryCatalog : ComposablePartCatalog
    {
        private readonly AggregateCatalog _catalog;
        private readonly DirectoryInfo _dirInfo;

        public string FullPath => _dirInfo.FullName;
        public override IQueryable<ComposablePartDefinition> Parts => _catalog.Parts;

        public SafeDirectoryCatalog(string directory)
        {
            _dirInfo = new DirectoryInfo(directory);
            _catalog = new AggregateCatalog();

            var files = _dirInfo.EnumerateFiles("*.dll", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var catalog = new SafeAssemblyCatalog(file.FullName);

                    //Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (catalog.Parts.ToList().Count > 0)
                        _catalog.Catalogs.Add(catalog);
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning($"Could not load '{file.FullName}': {ex}");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _catalog.Dispose();
        }
    }
}