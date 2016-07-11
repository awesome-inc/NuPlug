using System;
using System.Net;
using FluentAssertions;
using NuGet;
using NUnit.Framework;

namespace NuPlug
{
    internal class LearningTests
    {
        [Test, Description("NO_PROXY=*.company.com throws argument/regexexception")]
        public void NuGet_ProxyCache_should_work()
        {
            var uri = new Uri("https://nuget.org/api/v2/");
            0.Invoking(x => new WebProxy(uri).BypassList = new [] {"localhost","127.0.0.1", "*.acme.com"}).ShouldThrow<ArgumentException>();

            var sut = ProxyCache.Instance;
            sut.GetProxy(uri);
        }
    }
}