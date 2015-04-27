using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CK.FileFilter
{
    internal abstract class VirtualFileStorageDriver : IDisposable
    {
        internal protected readonly VirtualFileStorage Storage;
        internal protected readonly VirtualFileStorageDriver Parent;
        internal protected readonly string RootPath;

        internal VirtualFileStorageDriver( VirtualFileStorage storage )
        {
            Storage = storage;
            Parent = null;
            RootPath = String.Empty;
        }

        public VirtualFileStorageDriver( VirtualFileStorage storage, VirtualFileStorageDriver parent, string rootPath )
        {
            Storage = storage;
            Parent = parent;
            RootPath = rootPath;
            Storage.AddDriver( this );
        }

        public abstract Stream OpenRead( string relativePath );

        public abstract IEnumerable<string> EnumerateFiles( string relativePath );

        public abstract void Dispose();
    }
}
