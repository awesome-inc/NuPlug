using System;
using System.Diagnostics;
using FluentAssertions;
using NEdifis.Attributes;
using NEdifis.Diagnostics;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (TraceLogger))]
    // ReSharper disable InconsistentNaming
    internal class TraceLogger_Should
    {
        [Test]
        public void Log_to_trace()
        {
            var sut = new TraceLogger();
            using (var traceListener = new TestTraceListener { ActiveTraceLevel = TraceLevel.Verbose})
            {
                foreach (MessageLevel messageLevel in Enum.GetValues(typeof (MessageLevel)))
                {
                    var traceLevel = messageLevel.ToTraceLevel();
                    var message = $"message: {messageLevel} - {traceLevel}";
                    sut.Log(messageLevel, message);
                    traceListener.MessagesFor(traceLevel).Should().Contain(message);
                }
            }
        }

        [Test]
        public void Resolve_conflicts()
        {
            var sut = new TraceLogger();
            foreach (FileConflictResolution expected in Enum.GetValues(typeof (FileConflictResolution)))
            {
                sut.FileConflictResolution = expected;
                sut.ResolveFileConflict("message").Should().Be(expected);
            }
        }
    }
}