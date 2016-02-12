using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace CK.Globbing.Tests
{

    public class TestHelper
    {
        static string _solutionDir;

        public static string SolutionDir
        {
            get
            {
                if( _solutionDir == null ) InitalizePaths();
                return _solutionDir;
            }
        }


        private static void InitalizePaths()
        {
            string p = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            // Code base is like "file:///C:/Documents and Settings/Spi/Mes documents/Dev/CK-Package/CK.Package.Tests/bin/Debug/CK.Package.Tests.dll"
            Assert.That( p, Is.StringStarting( "file:///" ), "Code base must start with file:/// protocol." );

            p = p.Substring( 8 ).Replace( '/', System.IO.Path.DirectorySeparatorChar );

            p = Path.GetDirectoryName( p );
            p = Path.GetDirectoryName( p );
            p = Path.GetDirectoryName( p );
            p = Path.GetDirectoryName( p );

            _solutionDir = p + '\\';
        }


    }
}
