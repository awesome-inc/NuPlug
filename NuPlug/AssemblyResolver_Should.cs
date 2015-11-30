using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NEdifis.Attributes;
using NEdifis.Diagnostics;
using NUnit.Framework;

namespace NuPlug
{
    [TestFixtureFor(typeof (AssemblyResolver))]
    // ReSharper disable InconsistentNaming
    internal class AssemblyResolver_Should
    {
        [Test]
        public void Not_resolve_without_directories()
        {
            using (var sut = new AssemblyResolver())
            {
                var assembly = typeof (AssemblyResolver_Should).Assembly;
                var resolvedAssembly = sut.ResolveAssembly(this, new ResolveEventArgs(assembly.GetName().Name));
                Assert.IsNull(resolvedAssembly, "should resolve to null, because no directories to resolve from.");
            }
        }

        [Test]
        [TestCase(typeof (AssemblyResolver_Should))]
        [TestCase(typeof (TestCaseAttribute))]
        [TestCase(typeof(TestFixtureForAttribute))]
        public void Resolve_assemblies(Type typeToResolve)
        {
            using (var sut = new AssemblyResolver {TraceAlways = true})
            using (var ttl = new TestTraceListener {ActiveTraceLevel = TraceLevel.Verbose})
            {
                var assembly = typeToResolve.Assembly;
                var dir = assembly.GetDirectory();
                sut.Directories.Add(dir);

                var requesting = typeof (AssemblyResolver_Should).Assembly;
                var aName = assembly.GetName();
                var resolvedAssembly = sut.ResolveAssembly(this, new ResolveEventArgs(aName.Name, requesting));
                resolvedAssembly.GetName().Name.Should().Be(aName.Name);

                var msgs = ttl.MessagesFor(TraceLevel.Verbose).ToArray();
                msgs[0].Should().Be($"Requested to load '{aName.Name}' by '{requesting.FullName}'.");
                msgs[1].Should().StartWith($"Resolved '{aName.Name}, {aName.Version}' from '{assembly.GetLocation()}");
            }
        }
    }
}