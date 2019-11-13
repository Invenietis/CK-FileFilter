using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace CK.Globbing
{

    /// <summary>
    /// Defines the behavior for unmatched file names.
    /// </summary>
    public enum FileFilterMatchBehavior
    {
        /// <summary>
        /// Default behavior is to exclude not explicitly matched files.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Consider file names that do not explicitly match as being included.
        /// </summary>
        NoneIsIncluded,

        /// <summary>
        /// No unmatched files must appear: any file that do not match explicitly (be it in include 
        /// or in exclude) results in an <see cref="UnmatchedFileException"/>.
        /// </summary>
        NoneIsUnmatchedFileException
    }

    /// <summary>
    /// Groups source filters and targets one destination path.
    /// </summary>
    public class FileGroupTarget : NotifierBase
    {
        string _target;
        FileFilterMatchBehavior _matchBehavior;

        /// <summary>
        /// Initializes a new FileGroupTarget.
        /// </summary>
        public FileGroupTarget()
        {
            _target = "/";
            Filters = new ObservableCollection<FileNameFilter>();
        }

        public FileGroupTarget( XElement e )
            : this()
        {
            FromXml( e );
        }

        /// <summary>
        /// Initializes a new FileGroupTarget object with a custom initializer.
        /// </summary>
        /// <param name="initializer">A custom FileGroupTarget initializer.</param>
        public FileGroupTarget( Action<FileGroupTarget> initializer )
            : this()
        {
            initializer( this );
        }

        /// <summary>
        /// Gets or sets the target path of the selected files.
        /// This is associated to the <see cref="FileNameFilter"/>.<see cref="FileNameFilter.Root"/> property.
        /// It defaults to "/" that is the root of the target.
        /// </summary>
        public string Target
        {
            get { return _target; }
            set
            {
                if( String.IsNullOrWhiteSpace( value ) ) value = "/";
                if( _target != value )
                {
                    _target = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the behavior regarding unmatched file names.
        /// </summary>
        public FileFilterMatchBehavior MatchBehavior
        {
            get { return _matchBehavior; }
            set
            {
                if( _matchBehavior != value )
                {
                    _matchBehavior = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="FileNameFilter"/>.
        /// </summary>
        public ObservableCollection<FileNameFilter> Filters { get; private set; }

        internal XElement ToXml()
        {
            var e = new XElement( "FileGroupTarget", new XAttribute( "Target", Target ) );
            if( _matchBehavior != FileFilterMatchBehavior.Default )
            {
                e.Add( new XAttribute( "MatchBehavior", _matchBehavior.ToString() ) );
            }
            foreach( var filter in Filters ) e.Add( filter.ToXml() );
            return e;
        }

        internal void FromXml( XElement e )
        {
            Target = e.Attribute( "Target" ).Value;
            string matchBehavior = e.Attribute( "MatchBehavior" )?.Value;
            if( matchBehavior != null && Enum.TryParse<FileFilterMatchBehavior>( e.Attribute( "MatchBehavior" ).Value, out var val ) )
            {
                MatchBehavior = val;
            }
            else
            {
                MatchBehavior = FileFilterMatchBehavior.Default;
            }
            Filters.Clear();
            foreach( var i in e.Elements( "Filter" )
                .Select( f => new FileNameFilter( f ) ) )
            {
                Filters.Add( i );
            }
        }

        /// <summary>
        /// Encapsulates the result of <see cref="FileGroupTarget.IncludedFiles"/> lookup.
        /// </summary>
        public struct Result
        {
            internal Result( string filePath, string removableRoot, string filterRootedPath )
            {
                Debug.Assert( removableRoot != null && filePath == removableRoot + filterRootedPath );
                FilePath = filePath;
                RemovableRoot = removableRoot;
                FinalFilePath = filterRootedPath;
            }

            /// <summary>
            /// The file path relative to the parameter root given in <see cref="FileGroupTarget.IncludedFiles"/>.
            /// When no source mapping exists (that is currently the case), it is the concatenation of <see cref="RemovableRoot"/> and <see cref="FinalFilePath"/>
            /// Never null nor empty.
            /// </summary>
            public readonly string FilePath;

            /// <summary>
            /// The root that must be removed from <see cref="FilePath"/>.
            /// Never null but may be empty.
            /// </summary>
            /// <remarks>
            /// This corresponds to the <see cref="FileNameFilter.Root"/> when the file is issued from such a filter.
            /// </remarks>
            public readonly string RemovableRoot;

            /// <summary>
            /// The file path that must be considered.
            /// </summary>
            public readonly string FinalFilePath;
        }

        /// <summary>
        /// Challenge a relative path. This method ignores the <see cref="MatchBehavior"/> property.
        /// </summary>
        /// <param name="rootedFilePath">The relative path to challenge.</param>
        /// <param name="r">Result of the match. If a rule has been found, it contains the <see cref="FileNameFilter.Root"/> as its <see cref="Result.RemovableRoot"/>.</param>
        /// <returns>The <see cref="FilterMatchResult"/>.</returns>
        public FilterMatchResult IncludedFile( string rootedFilePath, out Result r )
        {
            FilterMatchResult match = FilterMatchResult.None;
            string filterRoot = null;
            foreach( FileNameFilter filter in Filters )
            {
                match = filter.FilePathMatch( rootedFilePath );
                if( match != FilterMatchResult.None )
                {
                    filterRoot = filter.Root ?? String.Empty;
                    break;
                }
            }
            if( filterRoot == null )
            {
                Debug.Assert( match == FilterMatchResult.None );
                r = new Result( rootedFilePath, String.Empty, rootedFilePath );
            }
            else r = new Result( rootedFilePath, filterRoot, rootedFilePath.Substring( filterRoot.Length ) );
            return match;
        }

        /// <summary>
        /// Returns the list of file paths included by this FileGroupTarget.
        /// It can throw an <see cref="UnmatchedFileException"/> if <see cref="MatchBehavior"/> is <see cref="FileFilterMatchBehavior.NoneIsUnmatchedFileException"/>.
        /// </summary>
        /// <param name="root">The path of the directory into which we must look for included file paths.</param>
        /// <returns>The list of included file paths relative to <paramref name="root"/>.</returns>
        public IEnumerable<Result> IncludedFiles( string root, IVirtualFileStorage fs )
        {
            if( string.IsNullOrWhiteSpace( root ) ) throw new ArgumentException( "root must be not null nor whitespace", "root" );
            if( fs == null ) throw new ArgumentNullException( "fs" );
            foreach( string fullName in fs.EnumerateFiles( root ) )
            {
                FilterMatchResult m = FilterMatchResult.None;
                string filePath = fullName.Substring( root.Length );
                string filterRoot = null;
                foreach( FileNameFilter filter in Filters )
                {
                    m = filter.FilePathMatch( filePath );
                    if( m != FilterMatchResult.Excluded )
                    {
                        filterRoot = filter.Root ?? String.Empty;
                    }
                    if( m != FilterMatchResult.None ) break;
                }
                if( m == FilterMatchResult.None && _matchBehavior == FileFilterMatchBehavior.NoneIsUnmatchedFileException )
                {
                    throw new UnmatchedFileException( fullName );
                }
                if( m == FilterMatchResult.Included || (m == FilterMatchResult.None && _matchBehavior == FileFilterMatchBehavior.NoneIsIncluded) )
                {
                    yield return new Result( filePath, filterRoot, filePath.Substring( filterRoot.Length ) );
                }
            }
        }

    }
}

