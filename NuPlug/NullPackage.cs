using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet;

namespace NuPlug
{
    internal class NullPackage : LocalPackage
    {
        public NullPackage(string id, SemanticVersion version)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (version == null) throw new ArgumentNullException(nameof(version));
            Id = id;
            Version = version;
            DependencySets = Enumerable.Empty<PackageDependencySet>();
        }

        public override Stream GetStream()
        {
            var nuspec = $"<?xml version=\"1.0\"?><package><metadata><id>{Id}</id><version>{Version}</version><authors>None</authors><description>None</description></metadata></package>";
            return GenerateStreamFromString(nuspec);
        }

        public override void ExtractContents(IFileSystem fileSystem, string extractPath)
        {
        }

        protected override IEnumerable<IPackageFile> GetFilesBase()
        {
            return Enumerable.Empty<IPackageFile>();
        }

        protected override IEnumerable<IPackageAssemblyReference> GetAssemblyReferencesCore()
        {
            return Enumerable.Empty<IPackageAssemblyReference>();
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}