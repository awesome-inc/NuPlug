using System.Diagnostics;
using FluentAssertions;
using NEdifis.Attributes;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (MessageLevelExtensions))]
    // ReSharper disable InconsistentNaming
    internal class MessageLevelExtensions_Should
    {
        [Test]
        [TestCase(MessageLevel.Debug, TraceLevel.Verbose)]
        [TestCase(MessageLevel.Error, TraceLevel.Error)]
        [TestCase(MessageLevel.Info, TraceLevel.Info)]
        [TestCase(MessageLevel.Warning, TraceLevel.Warning)]
        public void Convert_levels(MessageLevel input, TraceLevel expected)
        {
            input.ToTraceLevel().Should().Be(expected);
        }
    }
}