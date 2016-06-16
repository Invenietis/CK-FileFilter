using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using CK.Core;

namespace CK.Globbing
{
    public class VirtualFileStorage : IVirtualFileStorage
    {
        CKSortedArrayKeyList<VirtualFileStorageDriver,string> _drivers;
        VirtualFileStorageDriverRoot _root;
        internal bool _handleZip;
        internal bool _handleNupkg;
        
        #if DEBUG
        internal int _countOpenStream;
        #endif

        public VirtualFileStorage()
            : this( true, true )
        {
        }

        public VirtualFileStorage( bool handleZip, bool handleNupkg )
        {
            _handleZip = handleZip;
            _handleNupkg = handleNupkg;
            _drivers = new CKSortedArrayKeyList<VirtualFileStorageDriver, string>( v => v.RootPath, StringComparer.OrdinalIgnoreCase.Compare );
            _root = new VirtualFileStorageDriverRoot( this );
            _drivers.Add( _root );
        }

        public IEnumerable<string> EnumerateFiles( string fullDirectoryPath )
        {
            if( String.IsNullOrWhiteSpace( fullDirectoryPath ) ) throw new ArgumentException();
            fullDirectoryPath = FileUtil.NormalizePathSeparator( fullDirectoryPath, true );

            VirtualFileStorageDriver closestDriver = FindClosestDriver( fullDirectoryPath );
            return closestDriver.EnumerateFiles( fullDirectoryPath.Substring( closestDriver.RootPath.Length ) );
        }

        public Stream OpenRead( string fullPath )
        {
            if( String.IsNullOrWhiteSpace( fullPath ) ) throw new ArgumentException();
            fullPath = FileUtil.NormalizePathSeparator( fullPath, false );

            // Early test: if it is a file, we shortcut the process.
            if( File.Exists( fullPath ) ) return _root.DoOpen( fullPath, FileAccess.Read );

            VirtualFileStorageDriver closestDriver = FindClosestDriver( fullPath );
            return closestDriver.OpenRead( fullPath.Substring( closestDriver.RootPath.Length ) );
        }

        private VirtualFileStorageDriver FindClosestDriver( string fullPath )
        {
            VirtualFileStorageDriver closestDriver;
            int idxDriver = _drivers.IndexOf( fullPath );
            Debug.Assert( idxDriver != 0, "Since fullPath is not empty and the 0 is the root: the dichotomic search can not have found the root." );
            if( idxDriver > 0 )
            {
                // Pathological case: the path designates a container:
                // we redirect the action to its Parent.
                closestDriver = _drivers[idxDriver];
                closestDriver = closestDriver.Parent;
            }
            else
            {
                idxDriver = ~idxDriver;
                Debug.Assert( idxDriver > 0, "Since fullPath is not empty." );
                for( ; ; )
                {
                    var d = _drivers[--idxDriver];
                    if( idxDriver == 0 || fullPath.StartsWith( d.RootPath, StringComparison.OrdinalIgnoreCase ) )
                    {
                        closestDriver = d;
                        break;
                    }
                }
            }
            return closestDriver;
        }

        internal void AddDriver( VirtualFileStorageDriver d )
        {
            _drivers.Add( d );
        }

        public void Dispose()
        {
            int i = _drivers.Count;
            while( --i > 0 ) _drivers[i].Dispose();
            _drivers.Clear();
            //#if DEBUG
            //Debug.Assert( _countOpenStream == 0 );
            //#endif
        }

    }
}
