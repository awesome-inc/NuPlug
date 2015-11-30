using System;
using System.Diagnostics;
using NuGet;

namespace NuPlug
{
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageLevel), messageLevel, null);
            }
        }
    }
}