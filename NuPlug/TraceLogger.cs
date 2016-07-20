using System.Diagnostics;
using NuGet;

namespace NuPlug
{
    /// <summary>
    /// An <see cref="ILogger"/> implementation for <see cref="Trace"/>.
    /// </summary>
    public class TraceLogger : ILogger
    {
        /// <summary>
        /// Returns <see cref="FileConflictResolution"/>
        /// </summary>
        public FileConflictResolution ResolveFileConflict(string message)
        {
            return FileConflictResolution;
        }

        /// <summary>
        /// Logs the specified message to <see cref="Trace"/>.
        /// </summary>
        /// <param name="level">The trace level</param>
        /// <param name="message">The message</param>
        /// <param name="args">The args</param>
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

        /// <summary>
        /// The <see cref="NuGet.FileConflictResolution"/>. Default is <see cref="NuGet.FileConflictResolution.Ignore"/>
        /// </summary>
        public FileConflictResolution FileConflictResolution { get; set; } = FileConflictResolution.Ignore;
    }
}