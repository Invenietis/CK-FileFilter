using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Cake.Core.Annotations;
using CK.Globbing;

namespace Cake.CK.Pack
{
    [CakeAliasCategory( "CKPack" )]
    public static class CKPackAliases
    {
        [CakeMethodAlias]
        public static string Pack( string rootDirectory, string outputDirectory, string outputFileName, IEnumerable<FileGroupTarget> targets )
        {
            return Pack( rootDirectory, BuildOutputFileName( outputDirectory, outputFileName ), targets );
        }

        [CakeMethodAlias]
        public static string Pack( string configurationFile, string outputDirectory, string outputFileName )
        {
            if( !File.Exists( configurationFile ) ) throw new FileNotFoundException( $"File {configurationFile}: not found" );

            var xDoc = XDocument.Load( configurationFile );

            var targets = xDoc.Descendants("FileGroupTarget");

            if( targets.Count() == 0 ) throw new FormatException( "Configuration file must contain one or more FileGroupTarget" );

            var list = targets.Select(x => new FileGroupTarget(x));

            return Pack( ExtractRootDirectoryFromConfigurationFile( configurationFile ), BuildOutputFileName( outputDirectory, outputFileName ), list );
        }

        private static string BuildOutputFileName( string outputDirectory, string outputFileName )
        {
            return Path.Combine( outputDirectory, outputFileName );
        }

        private static string ExtractRootDirectoryFromConfigurationFile( string filePath )
        {
            var lastSeparator = filePath.LastIndexOf("\\");

            return filePath.Substring( 0, lastSeparator + 1 );
        }

        private static string Pack( string rootDirectory, string outputFilePath, IEnumerable<FileGroupTarget> targets )
        {
            if( File.Exists( outputFilePath ) ) File.Delete( outputFilePath );

            using( IVirtualFileStorage fs = new VirtualFileStorage( true, false ) )
            {
                using( var zip = ZipFile.Open( outputFilePath, ZipArchiveMode.Create ) )
                {
                    foreach( var t in targets )
                    {
                        foreach( var f in t.IncludedFiles( rootDirectory, fs ) )
                        {
                            var sourceFile = Path.Combine(rootDirectory, f.FilePath);
                            var destinationFile = Path.Combine(t.Target.Substring(1), f.FinalFilePath);

                            zip.CreateEntryFromFile( sourceFile, destinationFile );
                        }
                    }
                }
            }

            return outputFilePath;
        }
    }
}
