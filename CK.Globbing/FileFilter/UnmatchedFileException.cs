using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Runtime.Serialization;

namespace CK.Globbing
{
    [Serializable]
    public class UnmatchedFileException : Exception
    {
        internal UnmatchedFileException( string fileName )
            : base( String.Format( "Unmatched file name: {0}", fileName ) )
        {
            Data.Add( "UnmatchedFile", fileName );
        }

        public string UnmatchedFile 
        { 
            get { return (string)Data["UnmatchedFile"]; } 
        }

    }
}
