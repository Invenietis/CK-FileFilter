using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using CK.Core;
using System.IO.Compression;
using static CK.Testing.BasicTestHelper;

namespace CK.Globbing.Tests
{
    [TestFixture]
    public class ZipTests
    {
        static string ZipTestsFolder = Path.Combine( TestHelper.TestProjectFolder, @"ZipTests\" );

        [Test]
        public void writing_and_reading_a_very_small_zip_file()
        {
            string path = Path.Combine( ZipTestsFolder, @"writing_a_very_small_zip_file.zip" );
            var bytesWrite = new byte[] {   30, 21, 36, 4,  248, 5,
                                            60, 11, 52, 43, 186, 200,
                                            0, 91, 68, 234, 215 };
            
            File.Delete( path );
            using( var file = File.OpenWrite( path ) )
            using( var zip = new ZipArchive( file, ZipArchiveMode.Create ) )
            {
                var c = zip.CreateEntry( "SimpleEntry.cs" );
                using( var content = c.Open() )
                {
                    content.Write( bytesWrite, 0, bytesWrite.Length );
                }
            }
            using( var file = File.OpenRead( path ) )
            using( var zip = new ZipArchive( file, ZipArchiveMode.Read ) )
            {
                var c = zip.GetEntry( "SimpleEntry.cs" );
                using( var content = c.Open() )
                {
                    var bytesRead = new byte[100];
                    Assert.That( content.Read( bytesRead, 0, bytesRead.Length ) == bytesWrite.Length );
                }
            }
            File.Delete( path );
        }

        [Test]
        public void transparently_enumerating_files_from_a_zip()
        {
            using( var s = new VirtualFileStorage() )
            {
                string root = Path.Combine( ZipTestsFolder, "Folder/FolderHasBeenZipped.zip" );
                var fInZip = s.EnumerateFiles( root ).ToList();
                ContainsFolderHasBeenZipped( fInZip, root );
            }
        }

        [Test]
        public void transparently_enumerating_files_from_a_zip_is_recursive()
        {
            using( var s = new VirtualFileStorage() )
            {
                {
                    string root = Path.Combine( ZipTestsFolder, "Folder" );

                    var fInFolder = s.EnumerateFiles( root ).ToList();
                    ContainsFolder( fInFolder, root, hasFolderHasBeenZipped:true );
                }
                {
                    string root = ZipTestsFolder;

                    var fInFolder = s.EnumerateFiles( root ).ToList();
                    ContainsZipTests( fInFolder, root );
                }
            }
        }

        [Test]
        public void transparently_enumerating_files_from_inside_a_zip()
        {
            using( var s = new VirtualFileStorage() )
            {
                {
                    string root = Path.Combine( ZipTestsFolder, "Folder/FolderHasBeenZipped.zip/Folder" );

                    var fInZip = s.EnumerateFiles( root ).ToList();
                    ContainsFolder( fInZip, root, hasFolderHasBeenZipped: false );
                }
                {
                    string root = Path.Combine( ZipTestsFolder, "src.zip/Bootstrapper" );

                    var fInZip = s.EnumerateFiles( root ).ToList();
                    ContainsBootstrapper( fInZip, root );
                }
            }
        }

        [Test]
        public void reading_file_data_can_be_transparently_done_from_a_zip_file()
        {
            using( var s = new VirtualFileStorage() )
            {
                {
                    string file = Path.Combine( ZipTestsFolder, "Folder/AnotherTextInFolder.txt" );
                    string text = ReadText( s, file );
                    Assert.That( text == "A" );
                }
                {
                    string file = Path.Combine( ZipTestsFolder, "Folder/Sub/NewDoc.txt" );
                    string text = ReadText( s, file );
                    Assert.That( text == "NewDoc" );
                }
                {
                    string file = Path.Combine( ZipTestsFolder, "Folder/FolderHasBeenZipped.zip/ANewTextDocument.txt" );
                    string text = ReadText( s, file );
                    Assert.That( text == "A2" );
                }
                {
                    string file = Path.Combine( ZipTestsFolder, "Folder/FolderHasBeenZipped.zip/Folder/Sub/NewDoc.txt" );
                    string text = ReadText( s, file );
                    Assert.That( text == "NewDoc" );
                }
                {
                    string file = Path.Combine( ZipTestsFolder, "src.zip/Bootstrapper/DataServicePackage.cs" );
                    string text = ReadText( s, file );
                    Assert.That( text == "File is DataServicePackage" );
                }
            }
        }

        string ReadText( IVirtualFileStorage s, string file )
        {
            using( var stream = s.OpenRead( file ) )
            using( var reader = new StreamReader( stream ) )
            {
                return reader.ReadToEnd();
            }
        }

        void ContainsFolderHasBeenZipped( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"ANewTextDocument.txt" ) );
            Assert.That( files.Contains( prefix + @"TextDocument.txt" ) );
            ContainsFolder( files, prefix + "Folder", hasFolderHasBeenZipped: false );
        }

        void ContainsFolder( IEnumerable<string> files, string prefix, bool hasFolderHasBeenZipped )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"Sub\NewDoc.txt" ) );
            Assert.That( files.Contains( prefix + @"AnotherTextInFolder.txt" ) );
            Assert.That( files.Contains( prefix + @"TextInFolder.txt" ) );

            if( hasFolderHasBeenZipped )
            {
                Assert.That( files.Contains( prefix + @"FolderHasBeenZipped.zip" ) );
                ContainsFolderHasBeenZipped( files, prefix + @"FolderHasBeenZipped.zip" );
            }
            else
            {
                Assert.That( files.Contains( prefix + @"FolderHasBeenZipped.zip" ) == false );
            }
        }

        void ContainsBootstrapper( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"Properties\AssemblyInfo.cs" ) );
            Assert.That( files.Contains( prefix + @"NuGetResources.resx" ) );
            Assert.That( files.Contains( prefix + @"Bootstrapper.csproj" ) );
            Assert.That( files.Contains( prefix + @"DataServicePackage.cs" ) );
        }

        void ContainsPackageManagerUIConverter( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"BooleanToVisibilityConverter.cs" ) );
            Assert.That( files.Contains( prefix + @"CountToVisibilityConverter.cs" ) );
            Assert.That( files.Contains( prefix + @"DescriptionLabelConverter.cs" ) );
            Assert.That( files.Contains( prefix + @"FixUrlConverter.cs" ) );
        }

        void ContainsPackageManagerUI( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"packageicon.png" ) );
            Assert.That( files.Contains( prefix + @"IProgressWindowOpener.cs" ) );
            Assert.That( files.Contains( prefix + @"ISelectedProviderSettings.cs" ) );
            Assert.That( files.Contains( prefix + @"IUserNotifierServices.cs" ) );

            ContainsPackageManagerUIConverter( files, prefix + "Converter" );
        }

        void ContainsDialogServices( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"Properties\AssemblyInfo.cs" ) );
            Assert.That( files.Contains( prefix + @"DialogServices.csproj" ) );
            Assert.That( files.Contains( prefix + @"WindowSizePersistenceHelper.cs" ) );
            Assert.That( files.Contains( prefix + @"NativeMethods.cs" ) );
            ContainsPackageManagerUI( files, "PackageManagerUI" );
        }

        void ContainsSrcZip( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            ContainsBootstrapper( files, prefix + "Bootstrapper" );
        }

        void ContainsZipTests( IEnumerable<string> files, string prefix )
        {
            prefix = FileUtil.NormalizePathSeparator( prefix, true );
            Assert.That( files.Contains( prefix + @"ANewTextDocument.txt" ) );
            Assert.That( files.Contains( prefix + @"TextDocument.txt" ) );
            
            Assert.That( files.Contains( prefix + @"src.zip" ) );
            ContainsSrcZip( files, prefix + "src.zip" );

            ContainsFolder( files, prefix + "Folder", hasFolderHasBeenZipped: true );
        }

    }
}
