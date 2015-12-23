using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NuPlug
{
    internal class SafeAssemblyCatalog : ComposablePartCatalog
    {
        internal readonly Assembly Assembly;
        private readonly Func<Type, bool> _typeFilter;
        private readonly ReflectionContext _reflectionContext;

        private ComposablePartCatalog _innerCatalog;
        private List<ComposablePartDefinition> _parts;

        private ComposablePartCatalog InnerCatalog => _innerCatalog ?? (_innerCatalog = CreateCatalog());
        private IEnumerable<ComposablePartDefinition> PartsInternal => _parts ?? (_parts = GetParts());


        public SafeAssemblyCatalog(Assembly assembly, Func<Type, bool> typeFilter, ReflectionContext reflectionContext = null)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (typeFilter == null) throw new ArgumentNullException(nameof(typeFilter));
            Assembly = assembly;
            _typeFilter = typeFilter;
            _reflectionContext = reflectionContext;
        }

        public SafeAssemblyCatalog(string fullName, Func<Type, bool> typeFilter = null, ReflectionContext reflectionContext = null)
            : this(LoadAssembly(fullName), typeFilter ?? (type => type.IsPublic), reflectionContext)
        { 
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

        private ComposablePartCatalog CreateCatalog()
        {
            var types = SelectTypes();
            if (_reflectionContext != null)
                return new TypeCatalog(types, _reflectionContext);
            return new TypeCatalog(types);
        }

        private IEnumerable<Type> SelectTypes()
        {
            return GetLoadableTypes(Assembly)
                .Where(_typeFilter);
        }

        private List<ComposablePartDefinition> GetParts()
        {
            var parts = new List<ComposablePartDefinition>();
            var types = SelectTypes();
            foreach (var type in types)
            {
                try
                {
                    var definition = AttributedModelServices.CreatePartDefinition(type, null, false);
                    if (definition != null)
                    {
                        parts.Add(definition);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
            return parts;
        }

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
                assemblyName = new AssemblyName { CodeBase = codeBase };
            }

            return Assembly.Load(assemblyName);
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            // http://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes        
            // http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Trace.WriteLine(e.Message);
                return e.Types.Where(t => t != null);
            }
        }
    }
}