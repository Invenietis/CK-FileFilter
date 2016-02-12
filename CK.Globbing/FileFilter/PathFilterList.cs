using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CK.Globbing
{
    /// <summary>
    /// Tri-state result of a file matching on multiple <see cref="PathFilter"/>.
    /// </summary>
    public enum FilterMatchResult
    {
        None,
        Included,
        Excluded
    }

    /// <summary>
    /// Describes a filter in terms of include/exclude patterns.
    /// The patterns are ordered. If many patterns match, the first is used.
    /// </summary>
    public class PathFilterList : FilteredObservableCollection<PathFilter>
    {
        /// <summary>
        /// Initializes a new <see cref="PathFilterList"/> with a predicate.
        /// </summary>
        /// <param name="filter">Filter to use. Can be null (it is then ignored).</param>
        public PathFilterList( Func<PathFilter, PathFilter> filter = null )
            : base( filter )
        {
        }

        /// <summary>
        /// Evaluates a path against the different <see cref="PathFilter"/>.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>A <see cref="FilterMatchResult"/> (can be done).</returns>
        public FilterMatchResult FilePathMatch( string fileName )
        {
            for( int i = 0; i < Count; ++i )
            {
                PathFilter f = Items[i];
                if( f.Regex.IsMatch( fileName ) ) return f.Include ? FilterMatchResult.Included : FilterMatchResult.Excluded;
            }
            return FilterMatchResult.None;
        }
    }
}
