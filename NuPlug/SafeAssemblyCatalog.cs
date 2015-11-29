using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace NuPlug
{
    internal class SafeAssemblyCatalog : ComposablePartCatalog
    {
        private readonly Assembly _assembly;
        private ComposablePartCatalog _innerCatalog;
        private volatile List<ComposablePartDefinition> _parts;

        public SafeAssemblyCatalog(string fullName)
        {
            _assembly = LoadAssembly(fullName);
        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            return InnerCatalog.GetExports(definition);
        }

        public override IEnumerator<ComposablePartDefinition> GetEnumerator()
        {
            return PartsInternal.GetEnumerator();
        }

        protected override void Dispose(bool disposing)
        {
            _innerCatalog?.Dispose();
            base.Dispose(disposing);
        }

        private ComposablePartCatalog InnerCatalog => _innerCatalog 
            ?? (_innerCatalog = new TypeCatalog(_assembly.GetLoadableTypes()));

        private static Assembly LoadAssembly(string codeBase)
        {
            if (string.IsNullOrWhiteSpace(codeBase)) throw new ArgumentException(nameof(codeBase));
            AssemblyName assemblyName;

            try
            {
                assemblyName = AssemblyName.GetAssemblyName(codeBase);
            }
            catch (ArgumentException)
            {
                assemblyName = new AssemblyName {CodeBase = codeBase};
            }

            return Assembly.Load(assemblyName);
        }

        private IEnumerable<ComposablePartDefinition> PartsInternal
        {
            get
            {
                if (_parts == null)
                {
                    var collection = new List<ComposablePartDefinition>();
                    var types = _assembly.GetLoadableTypes();
                    foreach (var type in types)
                    {
                        try
                        {
                            var definition = AttributedModelServices.CreatePartDefinition(type, null, false);
                            if (definition != null)
                            {
                                collection.Add(definition);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }
                    }
                    _parts = collection;
                }
                return _parts;
            }
        }

    }
}