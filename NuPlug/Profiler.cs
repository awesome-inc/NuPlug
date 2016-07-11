#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NEdifis.Attributes;

namespace NuPlug
{
    [ExcludeFromConventions("Copied from https://blogs.msdn.microsoft.com/shawnhar/2009/07/07/profiling-with-stopwatch/")]
    public class Profiler
    {
        public static readonly IDictionary<string,Profiler> AllProfilers = new Dictionary<string, Profiler>();

        private readonly string _name;
        private TimeSpan _elapsed;
        private Stopwatch _stopwatch;

        public Profiler(string name)
        {
            _name = name;
            AllProfilers[name] = this;
        }

        public void Start()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void Stop()
        {
            _elapsed += _stopwatch.Elapsed;
        }

        public void Print(TimeSpan totalTime)
        {
            Trace.WriteLine($"{_name}: {_elapsed} ({_elapsed.TotalSeconds*100/totalTime.TotalSeconds:F2}%)");
            _elapsed = TimeSpan.Zero;
        }
    }

    public struct ProfileMarker : IDisposable
    {
        readonly Profiler _profiler;
        public ProfileMarker(Profiler profiler)
        {
            _profiler = profiler;
            profiler.Start();
        }

        public void Dispose()
        {
            _profiler.Stop();
        }
    }
}
#endif