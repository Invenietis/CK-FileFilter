using System;
using System.Collections.Generic;
using System.IO;

namespace CK.Globbing
{
    public interface IVirtualFileStorage : IDisposable
    {
        /// <summary>
        /// Opens a stream. This may be inside a zip file or other container.
        /// </summary>
        /// <param name="fullPath">Full path to open.</param>
        /// <returns>An opened stream in read mode.</returns>
        Stream OpenRead( string fullPath );
        
        /// <summary>
        /// Retrieves the full path of all files below a given path.
        /// </summary>
        /// <param name="fullDirectoryPath">Root to filter.</param>
        /// <returns>Full paths.</returns>
        IEnumerable<string> EnumerateFiles( string fullDirectoryPath );
    }
}
