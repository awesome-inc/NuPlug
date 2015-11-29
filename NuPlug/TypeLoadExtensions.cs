using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NuPlug
{
    internal static class TypeLoadExtensions
    {
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
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