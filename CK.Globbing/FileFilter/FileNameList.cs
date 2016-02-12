using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;

namespace CK.Globbing
{
        /// <summary>
        /// Holds a compiled list of <see cref="Files"/> and an optional list of <see cref="Directories"/>. 
        /// These files and directories are strictly sorted using <see cref="OrdinalComparer.Default"/>.
        /// <see cref="Add"/> and <see cref="Remove"/> methods use and maintain this property.
        /// </summary>
        public class FileNameList
        {
            CKReadOnlyListOnIList<string> _files;
            CKReadOnlyListOnIList<string> _dir;

            /// <summary>
            /// Initializes a new <i>FileNameList</i> that may hold <see cref="Directories"/>.
            /// </summary>
            /// <param name="withDirectories">True to hold directories. Otherwise, <see cref="Directories"/>
            /// will remain null.</param>
            public FileNameList( bool withDirectories )
            {
                _files = new CKReadOnlyListOnIList<string>( Util.EmptyStringArray );
                if( withDirectories ) _dir = new CKReadOnlyListOnIList<string>( Util.EmptyStringArray );
            }


            /// <summary>
            /// Initializes a new <i>FileNameList</i> with a list of file names.
            /// The list can be (or must be) sorted thanks to <see cref="StringComparer.Ordinal"/>.
            /// </summary>
            /// <param name="files">List of file names.</param>
            /// <param name="prefix">Prefix if not null nor empty will be added to every file.</param>
            /// <param name="mustSort">True if sort must be called. False if the list is already sorted.</param>
            public FileNameList( IEnumerable<string> files, string prefix, bool mustSort )
            {
                if( mustSort ) files = files.OrderBy( s => s, StringComparer.Ordinal );
                Debug.Assert( mustSort || files.IsSortedStrict( StringComparer.Ordinal.Compare ) );
                _files = new CKReadOnlyListOnIList<string>( files.ToArray() );
                if( prefix != null && prefix.Length > 0 )
                    for( int i=0; i < _files.Count; ++i ) _files.Inner[i] = prefix + _files[i];
            }

            /// <summary>
            /// Sorted (thanks to <see cref="StringComparer.Ordinal"/>) list of file names.
            /// </summary>
            public IReadOnlyList<string> Files
            {
                get { return _files; }
            }

            /// <summary>
            /// Sorted (thanks to <see cref="StringComparer.Ordinal"/>) array of directory names. 
            /// Null if not explicitely specified at creation time.
            /// </summary>
            public IReadOnlyList<string> Directories
            {
                get { return _dir; }
            }

            /// <summary>
            /// Gets the index of a given file name in the <see cref="Files"/> array.
            /// </summary>
            /// <param name="fName">File name to find.</param>
            /// <returns>A negative value if not found (it is the result of the <see cref="Array.BinarySearch"/>).</returns>
            public int IndexOfFile( string fName )
            {
                return Array.BinarySearch( (string[])_files.Inner, fName, StringComparer.Ordinal );
            }

            /// <summary>
            /// Adds/combine another <see cref="FileNameList"/>. Duplicates are removed.
            /// </summary>
            /// <param name="x">Another <see cref="FileNameList"/> to combine to this object.</param>
            public void Add( FileNameList x )
            {
                _files.Inner = SortedStringArray.SortedArrayAdd( (string[])_files.Inner, (string[])x._files.Inner, StringComparer.Ordinal.Compare );
                if( _dir != null ) _dir.Inner = SortedStringArray.SortedArrayAdd( (string[])_dir.Inner, (string[])x._dir.Inner, StringComparer.Ordinal.Compare );
            }

            /// <summary>
            /// Removes (substracts) another <see cref="FileNameList"/>.
            /// </summary>
            /// <param name="x">Files and Directories that exist in <i>x</i> will be removed from
            /// this <see cref="FileNameList"/>.</param>
            public void Remove( FileNameList x )
            {
                _files.Inner = SortedStringArray.SortedArrayRemove( (string[])_files.Inner, (string[])x._files.Inner, StringComparer.Ordinal.Compare );
                if( _dir != null ) _dir.Inner = SortedStringArray.SortedArrayRemove( (string[])_dir.Inner, (string[])x._dir.Inner, StringComparer.Ordinal.Compare );
            }

            internal FileNameList( IEnumerable<string> files, IEnumerable<string> dir )
            {
                Debug.Assert( files.IsSortedStrict( StringComparer.Ordinal.Compare ) );
                Debug.Assert( dir == null || dir.IsSortedStrict( StringComparer.Ordinal.Compare ) );
                _files = new CKReadOnlyListOnIList<string>( files.ToArray() );
                if( dir != null ) _dir = new CKReadOnlyListOnIList<string>( dir.ToArray() );
            }

            public override string ToString()
            {
                return _files.ToString() + " / dir = " + _dir != null ? _dir.ToString() : "null";
            }
        }

    }

