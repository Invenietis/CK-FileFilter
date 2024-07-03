using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Versioning;
using Cake.Core;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;
using CK.Globbing;
using NUnit.Framework;
using static CK.Testing.BasicTestHelper;

namespace Cake.CK.Pack.Tests
{
    [TestFixture]
    public class Tests
    {
        ICakeContext _context;
        readonly string _testDirectory = System.IO.Path.Combine( TestHelper.TestProjectFolder, "FileTests");
        readonly string _outputDirectory = System.IO.Path.Combine( TestHelper.TestProjectFolder, "Output" );

        [SetUp]
        public void Setup()
        {
            _context = new CakeContext();
            _context.Environment.WorkingDirectory = _testDirectory;
            Directory.CreateDirectory( _outputDirectory );
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

            public DirectoryPath ApplicationRoot => throw new NotImplementedException();

            public ICakePlatform Platform => throw new NotImplementedException();

            public ICakeRuntime Runtime => throw new NotImplementedException();

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

            public ICakeArguments Arguments => throw new NotImplementedException();

            public ICakeEnvironment Environment => _env;

            public Core.IO.IFileSystem FileSystem => throw new NotImplementedException();

            public Core.IO.IGlobber Globber => throw new NotImplementedException();

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

            public IToolLocator Tools { get; }

            public ICakeDataResolver Data => throw new NotImplementedException();

            public ICakeConfiguration Configuration => throw new NotImplementedException();
        }

        private void CheckZipContent( string zipFilePath, IEnumerable<string> includedFiles )
        {
            using( var zipFile = ZipFile.OpenRead( zipFilePath ) )
            {
                var expectedFilesCount = includedFiles.Count();
                var foundFilesCount = zipFile.Entries.Count;

                Assert.That( expectedFilesCount == foundFilesCount, String.Format( "Created zip - {0} files expected, {1} found", expectedFilesCount, foundFilesCount ) );

                foreach( var f in includedFiles )
                {
                    var entry = zipFile.GetEntry(f);

                    Assert.That( entry != null, String.Format( "Created zip - {0}: not found", f ) );
                    Assert.That( f == entry.FullName, String.Format( "Created zip - Expected file: {0}, found: {1}", f, entry.FullName ) );
                }
            }
        }

        [Test]
        public void ExploreZip()
        {
            var config = System.IO.Path.Combine(_testDirectory, "configuration_zip.xml");
            var outputFile = System.IO.Path.Combine(_outputDirectory, "ExploreZip.zip");
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
            var config = System.IO.Path.Combine(_testDirectory, "configuration.xml");
            var outputFile = System.IO.Path.Combine(_outputDirectory, "PackFromConfigurationFile.zip");
            var targets = CKPackAliases.GetTargetsFromConfigurationFile(_context, config);

            CKPackAliases.Pack( _context, targets, outputFile );

            Assert.That( File.Exists( outputFile ), "Created zip - Output file not found" );

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

            var outputFile = System.IO.Path.Combine(_outputDirectory, "PackFromList.zip");

            CKPackAliases.Pack( _context, targets, outputFile );

            Assert.That( File.Exists( outputFile ), "Created zip - Output file not found" );

            CheckZipContent( outputFile, new[] {
                "RootFile.txt",
                @"Target1\File1.txt",
                @"Target1\File2.txt",
                @"Target3\File5.txt"
            } );
        }
    }
}
