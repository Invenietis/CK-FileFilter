using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cake.Common.IO.Paths;
using Cake.Core;
using Cake.Core.Annotations;
using CK.Core;
using CK.Globbing;

namespace Cake.CK.Pack
{
    [CakeAliasCategory( "CKPack" )]
    public static class CKPackAliases
    {
        [CakeMethodAlias]
        public static void Pack( this ICakeContext context, IEnumerable<FileGroupTarget> targets, string outputFile, bool handleZip = false, bool handleNuPkg = false )
        {
            if( context == null ) throw new ArgumentNullException( "context" );

            var rootDirectory = FileUtil.NormalizePathSeparator(context.Environment.WorkingDirectory.FullPath, true);

            if( File.Exists( outputFile ) ) File.Delete( outputFile );

            using( IVirtualFileStorage fs = new VirtualFileStorage( handleZip, handleNuPkg ) )
            {
                using( var zip = ZipFile.Open( outputFile, ZipArchiveMode.Create ) )
                {
                    foreach( var t in targets )
                    {
                        foreach( var f in t.IncludedFiles( rootDirectory, fs ) )
                        {
                            var sourceFile = Path.Combine(rootDirectory, f.FilePath);
                            var destinationFile = Path.Combine(t.Target.Substring(1), f.FinalFilePath);

                            using( var entry = zip.CreateEntry( destinationFile ).Open() )
                            {
                                fs.OpenRead( sourceFile ).CopyTo( entry );
                            }
                        }
                    }
                }
            }
        }

        [CakeMethodAlias]
        public static IEnumerable<FileGroupTarget> GetTargetsFromConfigurationFile( this ICakeContext context, string configurationFile )
        {
            if( context == null ) throw new ArgumentNullException( "context" );

            if( !File.Exists( configurationFile ) ) throw new FileNotFoundException( $"File {configurationFile}: not found" );

            var xDoc = XDocument.Load( configurationFile );

            var targets = xDoc.Descendants("FileGroupTarget");

            if( targets.Count() == 0 ) throw new FormatException( "Configuration file must contain one or more FileGroupTarget" );

            var list = targets.Select(x => new FileGroupTarget(x));

            return list;
        }
    }
}
