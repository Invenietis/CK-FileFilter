using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CK.Core;

namespace CK.FileFilter
{
    internal class VirtualFileStorageDriverRoot : VirtualFileStorageDriver
    {
        internal VirtualFileStorageDriverRoot( VirtualFileStorage storage )
            : base( storage )
        {
        }

        internal Stream DoOpen( string path, FileAccess mode )
        {
            #if DEBUG
            Stream s = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
            return new TrackedStream( Storage, s );
            #else
            return new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
            #endif
        }

        public override Stream OpenRead( string relativePath )
        {
            if( File.Exists( relativePath ) ) return DoOpen( relativePath, FileAccess.Read );

            VirtualFileStorageDriver d = FindDriverForPath( relativePath );
            if( d != null )
            {
                return d.OpenRead( relativePath.Substring( d.RootPath.Length ) );
            }
            throw new FileNotFoundException( "File not found.", RootPath + relativePath );
        }

        private VirtualFileStorageDriver FindDriverForPath( string relativePath )
        {
            VirtualFileStorageDriver d = null;
            string containerPath = relativePath;
            while( (containerPath = Path.GetDirectoryName( containerPath )) != null )
            {
                if( File.Exists( containerPath ) )
                {
                    d = CreateDriver( containerPath );
                    if( d != null ) break;
                }
            }
            return d;
        }

        private VirtualFileStorageDriver CreateDriver( string containerPath )
        {
            if( (Storage._handleZip && (containerPath.EndsWith( @".zip", StringComparison.OrdinalIgnoreCase ) || containerPath.EndsWith( @".zip\", StringComparison.OrdinalIgnoreCase )))
                || 
                (Storage._handleNupkg && (containerPath.EndsWith( @".nupkg", StringComparison.OrdinalIgnoreCase ) || containerPath.EndsWith( @".nupkg\", StringComparison.OrdinalIgnoreCase ))) )
            {
                return new VirtualFileStorageDriverRootZip( Storage, this, FileUtil.NormalizePathSeparator( containerPath, true ) );
            }
            return null;
        }

        public override IEnumerable<string> EnumerateFiles( string relativePath )
        {
            if( Directory.Exists( relativePath ) ) return EnumerateFilesForExistingDirectory( relativePath );
            VirtualFileStorageDriver d = CreateDriver( relativePath );
            if( d != null ) return d.EnumerateFiles( String.Empty );
            d = FindDriverForPath( relativePath );
            if( d != null ) return d.EnumerateFiles( relativePath.Substring( d.RootPath.Length ) );
            throw new DirectoryNotFoundException( String.Format( "Directory not found: {0}", relativePath ) );
        }

        IEnumerable<string> EnumerateFilesForExistingDirectory( string relativePath )
        {
            foreach( string f in Directory.EnumerateFiles( relativePath, "*", SearchOption.AllDirectories ) )
            {
                yield return f;
                VirtualFileStorageDriver d = CreateDriver( f );
                if( d != null )
                {
                    foreach( string fInZip in d.EnumerateFiles( String.Empty ) ) yield return fInZip;
                }
            }
        }
        
        public override void Dispose()
        {
        }
    }

}
