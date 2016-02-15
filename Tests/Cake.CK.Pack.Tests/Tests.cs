using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using CK.Globbing;
using NUnit.Framework;

namespace Cake.CK.Pack.Tests
{
    [TestFixture]
    public class Tests
    {
        private string testDirectory = @".\FileTests\";
        private string outputDirectory = @".\";

        private class CakeContext : ICakeContext
        {
            public ICakeArguments Arguments
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public ICakeEnvironment Environment
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IFileSystem FileSystem
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IGlobber Globber
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public ICakeLog Log
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IProcessRunner ProcessRunner
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IRegistry Registry
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void CheckZipContent( string zipFilePath, IEnumerable<string> includedFiles )
        {
            using( var zipFile = ZipFile.OpenRead( zipFilePath ) )
            {
                var expectedFilesCount = includedFiles.Count();
                var foundFilesCount = zipFile.Entries.Count;

                Assert.AreEqual( expectedFilesCount, foundFilesCount, String.Format( "Created zip - {0} files expected, {1} found", expectedFilesCount, foundFilesCount ) );

                foreach( var f in includedFiles )
                {
                    var entry = zipFile.GetEntry(f);

                    Assert.IsNotNull( entry, String.Format( "Created zip - {0}: not found", f ) );
                    Assert.AreEqual( f, entry.FullName, String.Format( "Created zip - Expected file: {0}, found: {1}", f, entry.FullName ) );
                }
            }
        }

        [Test]
        public void PackFromConfigurationFile()
        {
            var config = System.IO.Path.Combine(testDirectory, "configuration.xml");
            var outputFileName = "PackFromConfigurationFile.zip";

            var output = CKPackAliases.Pack( new CakeContext(), config, outputDirectory, outputFileName );

            var expectedOutput =  System.IO.Path.Combine( outputDirectory, outputFileName );

            Assert.AreEqual( output, expectedOutput, String.Format( "Created zip - Expected output path: {0}, get: {1}", expectedOutput, output ) );

            Assert.IsTrue( File.Exists( output ), "Created zip - Output file not found" );

            CheckZipContent( output, new[] {
                "RootFile.txt",
                @"Target1\File1.txt",
                @"Target1\File2.txt",
                @"Target3\File5.txt"
            } );
        }

        [Test]
        public void PackFromList()
        {
            var t1 = new FileGroupTarget() { Target = "/Target1" };
            var fn1 = new FileNameFilter() { Root = "ForTarget1" };
            var pf1 = new PathFilter(true, "/**.txt");
            t1.Filters.Add( fn1 );
            fn1.Filters.Add( pf1 );

            var t2 = new FileGroupTarget() { Target = "/Target2" };
            var fn2 = new FileNameFilter() { Root = "ForTarget2" };
            var pf2 = new PathFilter(false, "/**.txt");
            t2.Filters.Add( fn2 );
            fn2.Filters.Add( pf2 );

            var t3 = new FileGroupTarget() { Target = "/Target3" };
            var fn3 = new FileNameFilter() { Root = "ForTarget3" };
            var pf3e = new PathFilter(false, "/File6.txt");
            var pf3i = new PathFilter(true, "/**.txt");
            t3.Filters.Add( fn3 );
            fn3.Filters.Add( pf3e );
            fn3.Filters.Add( pf3i );

            var t4 = new FileGroupTarget() { Target = "/" };
            var fn4 = new FileNameFilter() { Root = "/" };
            var pf4 = new PathFilter(true, "/RootFile.txt");
            t4.Filters.Add( fn4 );
            fn4.Filters.Add( pf4 );

            var targets = new[] { t1, t2, t3, t4 };

            var outputFileName = "PackFromList.zip";

            var output = CKPackAliases.Pack( new CakeContext(), testDirectory, outputDirectory, outputFileName, targets );

            var expectedOutput = System.IO.Path.Combine( outputDirectory, outputFileName );

            Assert.AreEqual( output, expectedOutput, String.Format( "Created zip - Expected output path: {0}, get: {1}", expectedOutput, output ) );

            Assert.IsTrue( File.Exists( output ), "Created zip - Output file not found" );

            CheckZipContent( output, new[] {
                "RootFile.txt",
                @"Target1\File1.txt",
                @"Target1\File2.txt",
                @"Target3\File5.txt"
            } );
        }
    }
}
