using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// Helper class for getting and matching <see cref="FrameworkName"/> at runtime.
    /// </summary>
    public static class VersionHelper
    {
        /// <summary>
        /// Tries reading the <see cref="TargetFrameworkAttribute"/> from the specified assembly
        /// </summary>
        /// <param name="assembly">If null, defaults to <see cref="Assembly.GetEntryAssembly"/> or <see cref="Assembly.GetCallingAssembly"/></param>.
        public static FrameworkName GetTargetFramework(Assembly assembly = null)
        {
            var safeAssembly = assembly ?? (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly());
            var a = safeAssembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), false)
                .OfType<TargetFrameworkAttribute>().FirstOrDefault();
            return a != null ? new FrameworkName(a.FrameworkName) : null;
        }

        internal static string BestMatch(this Version version, IEnumerable<string> folderNames)
        {
            return folderNames
                .OrderBy(f => VersionDistance(version, VersionUtility.ParseFrameworkName(f)?.Version))
                .FirstOrDefault();
        }


        private static double VersionDistance(Version a, Version b)
        {
            if (a == null || b == null) return double.MaxValue;
            var dx = (a.Major - b.Major);
            var dy = (a.MajorRevision - b.MajorRevision);
            var dz = (a.Minor - b.Minor);
            var dw = (a.MinorRevision - b.MinorRevision);
            return 1000000 * dx * dx + 10000 * dy * dy + 100 * dz * dz + dw * dw;
        }
    }
}