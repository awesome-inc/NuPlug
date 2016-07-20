#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NEdifis.Attributes;

namespace NuPlug
{
    /// <summary>
    /// A simple profiler class.
    /// </summary>
    [ExcludeFromConventions("Copied from https://blogs.msdn.microsoft.com/shawnhar/2009/07/07/profiling-with-stopwatch/")]
    public class Profiler
    {
        /// <summary>
        /// All current profilers.
        /// </summary>
        public static readonly IDictionary<string,Profiler> AllProfilers = new Dictionary<string, Profiler>();

        private readonly string _name;
        private TimeSpan _elapsed;
        private Stopwatch _stopwatch;

        /// <summary>
        ///  Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="name">A (unique) name</param>
        public Profiler(string name)
        {
            _name = name;
            AllProfilers[name] = this;
        }

        /// <summary>
        /// Start profiling.
        /// </summary>
        public void Start()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stop profiling.
        /// </summary>
        public void Stop()
        {
            _elapsed += _stopwatch.Elapsed;
        }

        /// <summary>
        /// Print the time spent in profiling relative to <paramref name="totalTime"/> using <see cref="Trace"/> and <see cref="TraceLevel.Verbose"/>.
        /// </summary>
        /// <param name="totalTime"></param>
        public void Print(TimeSpan totalTime)
        {
            Trace.WriteLine($"{_name}: {_elapsed} ({_elapsed.TotalSeconds*100/totalTime.TotalSeconds:F2}%)");
            _elapsed = TimeSpan.Zero;
        }
    }

    /// <summary>
    /// A struct utilizing the disposable pattern on a <see cref="Profiler"/>
    /// </summary>
    public struct ProfileMarker : IDisposable
    {
        readonly Profiler _profiler;
        
        /// <summary>
        ///  Initializes a new instance of the <see cref="ProfileMarker"/> struct and starts profiling on the specified <paramref name="profiler"/>.
        /// </summary>
        public ProfileMarker(Profiler profiler)
        {
            _profiler = profiler;
            profiler.Start();
        }

        /// <summary>
        /// Stops profiling.
        /// </summary>
        public void Dispose()
        {
            _profiler.Stop();
        }
    }
}
#endif