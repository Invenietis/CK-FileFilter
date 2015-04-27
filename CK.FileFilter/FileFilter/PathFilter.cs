using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CK.Core;

namespace CK.FileFilter
{
    /// <summary>
    /// Describes an include/exclude pattern.
    /// </summary>
    public class PathFilter : NotifierBase
    {
        string _path;
        Regex _regex;

        /// <summary>
        /// Initializes an empty PathFilter object.
        /// </summary>
        public PathFilter()
        {
            _path = String.Empty;
        }

        /// <summary>
        /// Initializes a new PathFilter.
        /// </summary>
        /// <param name="include">Whether this PathFilter an include or exculde PathFilter.</param>
        /// <param name="path">The path pattern of this PathFilter.</param>
        public PathFilter( bool include, string path )
        {
            Include = include;
            _path = FileUtil.NormalizePathSeparator( path ?? String.Empty, false );
        }

        internal PathFilter( XElement e )
        {
            FromXml( e );
        }

        void FromXml( XElement e )
        {
            Include = e.Name.LocalName == "Include";
            Debug.Assert( Include || e.Name.LocalName == "Exclude" );
            Debug.Assert( !Include || e.Name.LocalName == "Include" );
            Path = e.Attribute( "Name" ).Value;
        }

        /// <summary>
        /// Gets or sets if this PathFilter is an include or exclude filter
        /// </summary>
        public bool Include { get; set; }

        /// <summary>
        /// Gets or sets the path pattern
        /// </summary>
        public string Path 
        {
            get { return _path; }
            set 
            {
                value = value ?? String.Empty;
                string path = FileUtil.NormalizePathSeparator( value, false );
                if( _path != path )
                {
                    _path = path;
                    _regex = null;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "Regex" );
                }
            }
        }
        
        /// <summary>
        /// Gets a regex equivalent to <see cref="Path"/> pattern
        /// </summary>
        public Regex Regex 
        {
            get { return _regex ?? (_regex = WildcardToRegex( _path )); } 
        }

        static Regex WildcardToRegex( string wildcard )
        {
            if( wildcard[wildcard.Length - 1] == '\\' )
                wildcard += "**";
            return new Regex( '^'
               + Regex.Escape( wildcard )
                .Replace( @"\*\*\\", ".*" ) //For recursive wildcards \**\, include the current directory.
                .Replace( @"\*\*", ".*" ) // For recursive wildcards that don't end in a slash e.g. **.txt would be treated as a .txt file at any depth
                .Replace( @"\*", @"[^\\]*(\\)?" ) // For non recursive searches, limit it any character that is not a directory separator
                .Replace( @"\?", "." ) // ? translates to a single any character
               + '$', RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture );
        }
    }
}
