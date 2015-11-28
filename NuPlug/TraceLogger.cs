using System.Diagnostics;
using NuGet;

namespace NuPlug
{
    public class TraceLogger : ILogger
    {
        public FileConflictResolution ResolveFileConflict(string message)
        {
            return FileConflictResolution;
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            var traceLevel = level.ToTraceLevel();
            switch (traceLevel)
            {
                case TraceLevel.Error: Trace.TraceError(message, args); break;
                case TraceLevel.Info: Trace.TraceInformation(message, args); break;
                case TraceLevel.Warning: Trace.TraceWarning(message, args); break;
                case TraceLevel.Verbose: Trace.WriteLine(string.Format(message, args)); break;
            }
        }

        public FileConflictResolution FileConflictResolution { get; set; } = FileConflictResolution.Ignore;
    }

    internal static class MessageLevelExtensions
    {
        internal static TraceLevel ToTraceLevel(this MessageLevel messageLevel)
        {
            switch (messageLevel)
            {
                case MessageLevel.Debug: return TraceLevel.Verbose;
                case MessageLevel.Error: return TraceLevel.Error;
                case MessageLevel.Info: return TraceLevel.Info;
                case MessageLevel.Warning: return TraceLevel.Warning;
            }
            return TraceLevel.Off;
        }
    }
}