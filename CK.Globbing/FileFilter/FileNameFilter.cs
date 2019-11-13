using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace CK.Globbing
{
    /// <summary>
    /// Describes a filter regarding the file names.
    /// </summary>
    public class FileNameFilter : NotifierBase
    {
        string	_root;
        PathFilterList _filter;

        /// <summary>
        /// Initiliazes an empty FileNameFilter object.
        /// </summary>
        public FileNameFilter()
        {
            _filter = new PathFilterList( NormalizePattern );
        }

        internal FileNameFilter( XElement e )
            : this()
        {
            FromXml( e );
        }

        /// <summary>
        /// Initializes a new FileNameFilter object with a custom initializer.
        /// </summary>
        /// <param name="initializer">A custom FileNameFilter initializer.</param>
        public FileNameFilter( Action<FileNameFilter> initializer )
            : this()
        {
            initializer( this );
        }

        /// <summary>
        /// A valid pattern is not null, not empty and is a relative path (leading / or a \ are removed).
        /// </summary>
        /// <param name="filter">The filter to check.</param>
        /// <returns>True if it is valid.</returns>
        static public PathFilter NormalizePattern( PathFilter filter )
        {
            if( filter == null ) throw new ArgumentNullException( "filter" );
            string pattern = filter.Path;
            if( String.IsNullOrWhiteSpace( pattern ) || pattern.IndexOfAny( Path.GetInvalidPathChars() ) > 0 )
            {
                throw new ArgumentException( "Must be a valid relative path.", "pattern" );
            }
            if( Path.GetPathRoot( pattern ).Length >= 2 )
            {
                throw new ArgumentException( "Must be a relative path.", "pattern" );
            }
            if( pattern[0] == Path.AltDirectorySeparatorChar || pattern[0] == Path.DirectorySeparatorChar )
            {
                return new PathFilter( filter.Include, pattern.Substring( 1 ) );
            }
            return new PathFilter( filter.Include, pattern );
        }

        /// <summary>
        /// Gets the <see cref="PathFilterList"/> associated with this FileNameFilter.
        /// </summary>
        public PathFilterList Filters { get { return _filter; } }

        /// <summary>
        /// Evaluates a path against the different <see cref="PathFilter"/> of the <see cref="Filters"/>.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>A <see cref="FilterMatchResult"/>.</returns>
        public FilterMatchResult FilePathMatch( string fileName )
        {
            if( Root == null ) return _filter.FilePathMatch( fileName );
            if( !fileName.StartsWith( Root, StringComparison.OrdinalIgnoreCase ) ) return FilterMatchResult.None;
            return _filter.FilePathMatch( fileName.Substring( Root.Length ) );
        }

        /// <summary>
        /// Patterns (either include or exclude) are relative to this directory.
        /// Defaults and normalized to null or normalized thanks to <see cref="FileUtil.NormalizePathSeparator"/>.
        /// </summary>
        public string Root
        {
            get { return _root; }
            set
            {
                if( String.IsNullOrWhiteSpace( value ) )
                {
                    value = null;
                }
                else
                {
                    value = FileUtil.NormalizePathSeparator( value, true );
                    if( value[0] == '\\' )
                    {
                        if( value.Length == 1 ) value = null;
                        else value = value.Substring( 1 );
                    }
                }

                if( _root != value )
                {
                    _root = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Removes all <see cref="Filters"/> but 
        /// preserves <see cref="Root"/>.
        /// </summary>
        public void Clear()
        {
            _filter.Clear();
        }

        internal XElement ToXml()
        {
            var e = new XElement( "Filter" );
            if( Root != null ) e.Add( new XAttribute( "Root", Root ) );
            foreach( var f in Filters ) e.Add( new XElement( f.Include ? "Include" : "Exclude", new XAttribute( "Name", f.Path ) ) );
            return e;
        }

        internal void FromXml( XElement e )
        {
            Root = e.Attribute( "Root" ) != null ? e.Attribute( "Root" ).Value : null;
            Filters.AddRange( from f in e.Elements() select new PathFilter( f ) );
        }
    }
}
