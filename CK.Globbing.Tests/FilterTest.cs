using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CK.Globbing.Tests
{
    [TestFixture]
    public class FilterTest
    {
        [Test]
        public void simple_PathFilterList_matches()
        {
            PathFilterList filter = new PathFilterList();
            {
                filter.Add( new PathFilter() { Include = true, Path = @"c:/essai1.txt" } );
                filter.Add( new PathFilter() { Include = true, Path = @"c:/essai2.txt" } );
                filter.Add( new PathFilter() { Include = true, Path = @"c:/essai3.txt" } );
                filter.Add( new PathFilter() { Include = false, Path = @"c:/essai4.txt" } );

                Assert.That( filter.FilePathMatch( @"c:\essai1.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai2.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai3.txt" ) == FilterMatchResult.Included );

                Assert.That( filter.FilePathMatch( @"c:\essai4.txt" ) == FilterMatchResult.Excluded );
                Assert.That( filter.FilePathMatch( @"c:\essai1.txt\essai.txt" ) == FilterMatchResult.None );
                Assert.That( filter.FilePathMatch( @"c:\essai1.txt\" ) == FilterMatchResult.None );
            }
            filter.Clear();
            {
                filter.Add( new PathFilter() { Include = true, Path = @"c:/essai1/*.txt" } );
                filter.Add( new PathFilter() { Include = false, Path = @"c:/essai1/essai1.txt" } );

                Assert.That( filter.FilePathMatch( @"c:\essai1\essai2.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai3.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai1.txt" ) == FilterMatchResult.Included );
            }
            filter.Clear();
            {
                filter.Add( new PathFilter() { Include = false, Path = @"c:/essai1/essai1.txt" } );
                filter.Add( new PathFilter() { Include = true, Path = @"c:/essai1/*.txt" } );

                Assert.That( filter.FilePathMatch( @"c:\essai1\essai2.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai3.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai1.txt" ) == FilterMatchResult.Excluded );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai2\essai1.txt" ) == FilterMatchResult.None );
            }
            filter.Clear();
            {
                filter.Add( new PathFilter() { Include = false, Path = @"c:/essai1/**/essai1.txt" } );
                filter.Add( new PathFilter() { Include = true, Path = @"c:/essai1/**.txt" } );

                Assert.That( filter.FilePathMatch( @"c:\essai1\essai2.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai2\essai3.txt" ) == FilterMatchResult.Included );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai2\essai1.txt" ) == FilterMatchResult.Excluded );
                Assert.That( filter.FilePathMatch( @"c:\essai1\essai1.txt" ) == FilterMatchResult.Excluded );
            }
        }
    }
}
