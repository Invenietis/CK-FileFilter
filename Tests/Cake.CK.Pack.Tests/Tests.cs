using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using CK.Core;
using CK.Globbing;
using NUnit.Framework;

namespace Cake.CK.Pack.Tests
{
    [TestFixture]
    public class Tests
    {
        private ICakeContext _context;
        private string _rootDirectory;
        private string _testDirectory;
        private string _outputDirectory;

        [SetUp]
        public void Setup()
        {
            _rootDirectory = Environment.CurrentDirectory;
            _testDirectory = Path.Combine( _rootDirectory, "FileTests");
            _outputDirectory = Path.Combine( _rootDirectory, "Output" );

            if( !Directory.Exists( _outputDirectory ) ) Directory.CreateDirectory( _outputDirectory );

            _context = new CakeContext();
            _context.Environment.WorkingDirectory = _testDirectory;
        }

        private class CakeEnvironment : ICakeEnvironment
        {
            private Core.IO.DirectoryPath _workingDir;

            public Core.IO.DirectoryPath WorkingDirectory
            {
                get
                {
                    return _workingDir;
                }

                set
                {
                    _workingDir = value;
                }
            }

            public Core.IO.DirectoryPath GetApplicationRoot()
            {
                throw new NotImplementedException();
            }

            public string GetEnvironmentVariable( string variable )
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, string> GetEnvironmentVariables()
            {
                throw new NotImplementedException();
            }

            public Core.IO.DirectoryPath GetSpecialPath( Core.IO.SpecialPath path )
            {
                throw new NotImplementedException();
            }

            public FrameworkName GetTargetFramework()
            {
                throw new NotImplementedException();
            }

            public bool Is64BitOperativeSystem()
            {
                throw new NotImplementedException();
            }

            public bool IsUnix()
            {
                throw new NotImplementedException();
            }
        }

        private class CakeContext : ICakeContext
        {
            private ICakeEnvironment _env = new CakeEnvironment();

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
                    return _env;
                }
            }

            public Core.IO.IFileSystem FileSystem
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Core.IO.IGlobber Globber
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

            public Core.IO.IProcessRunner ProcessRunner
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Core.IO.IRegistry Registry
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
        public void ExploreZip()
        {
            var config = Path.Combine(_testDirectory, "configuration_zip.xml");
            var outputFile = Path.Combine(_outputDirectory, "ExploreZip.zip");
            var targets = CKPackAliases.GetTargetsFromConfigurationFile(_context, config);

            CKPackAliases.Pack( _context, targets, outputFile, true );

            CheckZipContent( outputFile, new[] {
                @"Sub1\ZipFileSub1.txt",
                @"Sub1\Sub2\ZipFileSub2.txt",
                @"Sub1\Sub2\ZipFileSub2-2.txt"
            } );
        }

        [Test]
        public void PackFromConfigurationFile()
        {
            var config = Path.Combine(_testDirectory, "configuration.xml");
            var outputFile = Path.Combine(_outputDirectory, "PackFromConfigurationFile.zip");
            var targets = CKPackAliases.GetTargetsFromConfigurationFile(_context, config);

            CKPackAliases.Pack( _context, targets, outputFile );

            Assert.IsTrue( File.Exists( outputFile ), "Created zip - Output file not found" );

            CheckZipContent( outputFile, new[] {
                "RootFile.txt",
                @"Target1\File1.txt",
                @"Target1\File2.txt",
                @"Target1\ZipContent.zip",
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

            var outputFile = Path.Combine(_outputDirectory, "PackFromList.zip");

            CKPackAliases.Pack( _context, targets, outputFile );

            Assert.IsTrue( File.Exists( outputFile ), "Created zip - Output file not found" );

            CheckZipContent( outputFile, new[] {
                "RootFile.txt",
                @"Target1\File1.txt",
                @"Target1\File2.txt",
                @"Target3\File5.txt"
            } );
        }
    }
}
