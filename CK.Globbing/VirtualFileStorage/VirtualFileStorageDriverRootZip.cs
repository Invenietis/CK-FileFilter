using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using CK.Core;
using System.IO.Compression;

namespace CK.Globbing
{
    /// <summary>
    /// This can be used only for a file. It can not be used to handle recursive files (zip in a zip).
    /// </summary>
    internal class VirtualFileStorageDriverRootZip : VirtualFileStorageDriver
    {
        ZipArchive _file;

        public VirtualFileStorageDriverRootZip( VirtualFileStorage storage, VirtualFileStorageDriver parent, string rootPath )
            : base( storage, parent, rootPath )
        {
            Debug.Assert( rootPath.EndsWith( "\\" ) );
            _file = new ZipArchive( File.OpenRead( rootPath.Substring( 0, rootPath.Length - 1 ) ) ); 
        }

        public override Stream OpenRead( string relativePath )
        {
            ZipArchiveEntry e = _file.GetEntry( relativePath.Replace( '\\', '/' ) );
            if( e != null )
            {
                #if DEBUG
                Stream s = e.Open();
                return new TrackedStream( Storage, s );
                #else
                return e.OpenReader();
                #endif
            }
            throw new FileNotFoundException( "Unable to find file in " + RootPath, relativePath );
        }

        public override IEnumerable<string> EnumerateFiles( string relativePath )
        {
            return _file.Entries
                .Where( e => !String.IsNullOrEmpty( e.Name ) )
                .Select( e => FileUtil.NormalizePathSeparator( e.FullName, false ) )
                .Where( f => f.StartsWith( relativePath, StringComparison.OrdinalIgnoreCase ) )
                .Select( f => RootPath + f );
        }

        public override void Dispose()
        {
            _file.Dispose();
        }
    }
}
