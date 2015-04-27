using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.IO;

namespace CK.FileFilter.Tests
{
    [TestFixture]
    class FileNameFilterTests
    {
        [Test]
        public void invalid_patterns_throw_an_ArgumentException()
        {
            var filter = new FileNameFilter();
            Assert.Throws<ArgumentNullException>( () => filter.Filters.Add( null ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, "" ) ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, "  " ) ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, @"d:" ) ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, @"C:\Pouf" ) ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, @"\\server\share" ) ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, @"\\?\C:\Pouf" ) ) );
            Assert.Throws<ArgumentException>( () => filter.Filters.Add( new PathFilter( true, @"\\?\UNC\server\share" ) ) );
        }

        [Test]
        public void when_no_exlude_nor_include_matches_result_is_None()
        {
            FileGroupTarget target = new FileGroupTarget();
            FileNameFilter filter = new FileNameFilter();
            filter.Filters.AddRangeArray( new PathFilter( true, "**/*.inc" ), new PathFilter( false, "**/*.exc" ) );
            target.Filters.Add( filter );

            FileGroupTarget.Result r;
            {
                Assert.That( target.IncludedFile( "toto.exc", out r ), Is.EqualTo( FilterMatchResult.Excluded ) );
                Assert.That( target.IncludedFile( "a/b.toto.exc", out r ), Is.EqualTo( FilterMatchResult.Excluded ) );
                Assert.That( target.IncludedFile( "toto.inc", out r ), Is.EqualTo( FilterMatchResult.Included ) );
                Assert.That( target.IncludedFile( "a/b/b.toto.inc", out r ), Is.EqualTo( FilterMatchResult.Included ) );
                
                Assert.That( target.IncludedFile( String.Empty, out r ), Is.EqualTo( FilterMatchResult.None ) );
                Assert.That( target.IncludedFile( "murfn", out r ), Is.EqualTo( FilterMatchResult.None ) );
            }
        }

        class VF : IVirtualFileStorage
        {

            public System.IO.Stream OpenRead( string fullPath )
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> EnumerateFiles( string fullDirectoryPath )
            {
                var all = new[] 
                { 
                    "E:\\Dev\\CK-FileFilter\\.gitignore",
                    "E:\\Dev\\CK-FileFilter\\CK-FileFilter.sln",
                    "E:\\Dev\\CK-FileFilter\\CK-FileFilter.v12.suo",
                    "E:\\Dev\\CK-FileFilter\\LICENSE",
                    "E:\\Dev\\CK-FileFilter\\README.md",
                    "E:\\Dev\\CK-FileFilter\\.git\\config",
                    "E:\\Dev\\CK-FileFilter\\.git\\description",
                    "E:\\Dev\\CK-FileFilter\\.git\\HEAD",
                    "E:\\Dev\\CK-FileFilter\\.git\\index",
                    "E:\\Dev\\CK-FileFilter\\.git\\packed-refs",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\applypatch-msg.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\commit-msg.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\post-commit.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\post-receive.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\post-update.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\pre-applypatch.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\pre-commit.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\pre-push.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\pre-rebase.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\prepare-commit-msg.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\hooks\\update.sample",
                    "E:\\Dev\\CK-FileFilter\\.git\\info\\exclude",
                    "E:\\Dev\\CK-FileFilter\\.git\\logs\\HEAD",
                    "E:\\Dev\\CK-FileFilter\\.git\\logs\\refs\\heads\\master",
                    "E:\\Dev\\CK-FileFilter\\.git\\logs\\refs\\remotes\\origin\\HEAD",
                    "E:\\Dev\\CK-FileFilter\\.git\\objects\\pack\\pack-dea184a0c1f6403b7d975e6cfb43d73fbf64820b.idx",
                    "E:\\Dev\\CK-FileFilter\\.git\\objects\\pack\\pack-dea184a0c1f6403b7d975e6cfb43d73fbf64820b.pack",
                    "E:\\Dev\\CK-FileFilter\\.git\\refs\\heads\\master",
                    "E:\\Dev\\CK-FileFilter\\.git\\refs\\remotes\\origin\\HEAD",
                    "E:\\Dev\\CK-FileFilter\\.nuget\\packages.config",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\CK.FileFilter.csproj",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\NotifierBase.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\packages.config",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\bin\\Debug\\CK.Core.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\bin\\Debug\\CK.Core.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\bin\\Debug\\CK.Core.xml",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\bin\\Debug\\CK.FileFilter.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\bin\\Debug\\CK.FileFilter.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\FileGroupTarget.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\FileNameFilter.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\FileNameList.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\FilteredObservableCollection.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\PathFilter.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\PathFilterList.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\SortedStringArray.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\FileFilter\\UnmatchedFileException.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\CK.FileFilter.csproj.FileListAbsolute.txt",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\CK.FileFilter.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\CK.FileFilter.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\DesignTimeResolveAssemblyReferencesInput.cache",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\TemporaryGeneratedFile_036C0B5B-1481-4323-8D20-8F5ADCB23D92.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\TemporaryGeneratedFile_5937a670-0e60-4077-877b-f7221da3dda1.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\obj\\Debug\\TemporaryGeneratedFile_E7A71F73-0F8D-4B9B-B56E-8E70B10BC5D3.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\Properties\\AssemblyInfo.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\VirtualFileStorage\\IVirtualFileStorage.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\VirtualFileStorage\\TrackedStream.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\VirtualFileStorage\\VirtualFileStorage.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\VirtualFileStorage\\VirtualFileStorageDriver.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\VirtualFileStorage\\VirtualFileStorageDriverRoot.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter\\VirtualFileStorage\\VirtualFileStorageDriverRootZip.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\CK.FileFilter.Tests.csproj",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\CK.FileFilter.Tests.csproj.user",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\FileNameFilterTests.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\FilterTest.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\packages.config",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\TestHelper.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.Core.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.Core.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.Core.xml",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.FileFilter.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.FileFilter.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.FileFilter.Tests.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.FileFilter.Tests.dll.VisualState.xml",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\CK.FileFilter.Tests.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\nunit.framework.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\nunit.framework.xml",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\bin\\Debug\\TestResult.xml",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\CK.FileFilter.Tests.csproj.FileListAbsolute.txt",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\CK.FileFilter.Tests.csprojResolveAssemblyReference.cache",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\CK.FileFilter.Tests.dll",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\CK.FileFilter.Tests.pdb",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\DesignTimeResolveAssemblyReferencesInput.cache",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\TemporaryGeneratedFile_036C0B5B-1481-4323-8D20-8F5ADCB23D92.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\TemporaryGeneratedFile_5937a670-0e60-4077-877b-f7221da3dda1.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\obj\\Debug\\TemporaryGeneratedFile_E7A71F73-0F8D-4B9B-B56E-8E70B10BC5D3.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\Properties\\AssemblyInfo.cs",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\ANewTextDocument.txt",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\src.zip",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\TextDocument.txt",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\Folder\\AnotherTextInFolder.txt",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\Folder\\FolderHasBeenZipped.zip",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\Folder\\TextInFolder.txt",
                    "E:\\Dev\\CK-FileFilter\\CK.FileFilter.Tests\\ZipTests\\Folder\\Sub\\NewDoc.txt",
                    "E:\\Dev\\CK-FileFilter\\packages\\repositories.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\CK.Core.4.2.0.nupkg",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\lib\\net40\\CK.Core.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\lib\\net40\\CK.Core.pdb",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\lib\\net40\\CK.Core.xml",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\lib\\net45\\CK.Core.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\lib\\net45\\CK.Core.pdb",
                    "E:\\Dev\\CK-FileFilter\\packages\\CK.Core.4.2.0\\lib\\net45\\CK.Core.xml",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.2.6.4\\license.txt",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.2.6.4\\NUnit.2.6.4.nupkg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.2.6.4\\lib\\nunit.framework.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.2.6.4\\lib\\nunit.framework.xml",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\license.txt",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\NUnit.Runners.2.6.4.nupkg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\agent.conf",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\agent.log.conf",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\launcher.log.conf",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-agent-x86.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-agent-x86.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-agent.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-agent.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-console-x86.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-console-x86.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-console.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-console.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-editor.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-x86.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit-x86.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\nunit.framework.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\pnunit-agent.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\pnunit-agent.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\pnunit-launcher.exe",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\pnunit-launcher.exe.config",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\pnunit.framework.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\pnunit.tests.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\runpnunit.bat",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\test.conf",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\log4net.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit-console-runner.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit-gui-runner.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit.core.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit.core.interfaces.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit.uiexception.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit.uikit.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\nunit.util.dll",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Circles\\Failure.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Circles\\Ignored.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Circles\\Inconclusive.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Circles\\Skipped.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Circles\\Success.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Classic\\Failure.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Classic\\Ignored.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Classic\\Inconclusive.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Classic\\Skipped.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Classic\\Success.jpg",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Default\\Failure.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Default\\Ignored.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Default\\Inconclusive.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Default\\Skipped.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Default\\Success.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Visual Studio\\Failure.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Visual Studio\\Ignored.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Visual Studio\\Inconclusive.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Visual Studio\\SeriousWarning.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Visual Studio\\Skipped.png",
                    "E:\\Dev\\CK-FileFilter\\packages\\NUnit.Runners.2.6.4\\tools\\lib\\Images\\Tree\\Visual Studio\\Success.png"
                };

                return all.Where( f => f.StartsWith( fullDirectoryPath ) );
            }

            public void Dispose()
            {
            }
        }


        [Test]
        public void generates_files_from_this_solution()
        {
            foreach( var f in Directory.GetFiles( TestHelper.SolutionDir, "*", SearchOption.AllDirectories ) )
            {
                Console.WriteLine( @"""{0}"",", f.Replace( "\\", "\\\\" ) );
            }
        }

        [Test]
        public void selecting_files_from_this_solution()
        {
            using( var fs = new VF() )
            {
                FileGroupTarget fgt = new FileGroupTarget();
                
                fgt.MatchBehavior = FileFilterMatchBehavior.NoneIsIncluded;

                FileNameFilter filter = new FileNameFilter();
                filter.Filters.Add( new PathFilter( false, "/.git/" ) );
                filter.Filters.Add( new PathFilter( false, "/CK.FileFilter/File*/" ) );
                filter.Filters.Add( new PathFilter( true, "/packages/**/Circles/*.jpg" ) );
                filter.Filters.Add( new PathFilter( false, "/packages/**/*.jpg" ) );
                filter.Filters.Add( new PathFilter( false, "/**/ob?/" ) );
                fgt.Filters.Add( filter );

                IList<string> result = fgt.IncludedFiles( "E:\\Dev\\CK-FileFilter\\", fs ).Select( r => r.FilePath ).ToList();

                Assert.That( result.Count, Is.GreaterThan( 1 ) );

                Assert.That( result.Any( f => f.Contains( "\\.git\\" ) ), Is.False );
                
                Assert.That( result.Any( f => f.Contains( "\\CK.FileFilter\\File" ) ), Is.False );

                Assert.That( result.Count( f => f.EndsWith( ".jpg" ) ), Is.GreaterThan( 0 ) );
                Assert.That( result.Where( f => f.EndsWith( ".jpg" ) ).All( f => f.Contains( "\\Circles\\" ) ) );
                
                Assert.That( result.Any( f => f.Contains( "\\obj\\" ) ), Is.False );

                filter.Clear();
                filter.Filters.Add( new PathFilter( false, "**/NUnit.Runners.2.6.4/tools/lib/**/*/" ) );
                filter.Filters.Add( new PathFilter( true, "**/NUnit.Runners.2.6.4/tools/lib/" ) );

                fgt.MatchBehavior = FileFilterMatchBehavior.Default;
                result = fgt.IncludedFiles( "E:\\Dev\\CK-FileFilter\\", fs ).Select( r => r.FilePath ).ToList();
                
                Assert.That( result.Count(), Is.EqualTo( 8 ) );
            }
        }

    }
}
