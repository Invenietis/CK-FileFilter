using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;

namespace CK.FileFilter
{
    /// <summary>
    /// Offers efficient methods for sorted string arrays.
    /// CAUTION: it the arrays are not correctly sorted, behavior is impredictible.
    /// </summary>
    internal static class SortedStringArray
    {
        static string _sMark= "§mark (ref only)";

        internal static string[] SortedArrayAdd( string[] x1, string[] x2, Comparison<string> comparer )
        {
            Debug.Assert( x1 == null || x1.IsSortedStrict( comparer ) );
            Debug.Assert( x2 == null || x2.IsSortedStrict( comparer ) );

            if( x1 == null || x1.Length == 0 ) return x2;
            if( x2 == null || x2.Length == 0 ) return x1;

            string[] x = new string[x1.Length + x2.Length];

            int i1 = x1.Length - 1, i2 = x2.Length - 1, i = x1.Length + x2.Length;
            string s1 = x1[i1];
            string s2 = x2[i2];
            for( ; ; )
            {
                int cmp = comparer( s1, s2 );
                if( cmp > 0 )
                {
                    x[--i] = s1;
                    if( --i1 < 0 ) break;
                    s1 = x1[i1];
                }
                else if( cmp < 0 )
                {
                    x[--i] = s2;
                    if( --i2 < 0 ) break;
                    s2 = x2[i2];
                }
                else
                {
                    x[--i] = s1;
                    --i1;
                    --i2;
                    if( i1 < 0 || i2 < 0 ) break;
                    s1 = x1[i1];
                    s2 = x2[i2];
                }
            }
            if( ++i1 > 0 ) Array.Copy( x1, 0, x, (i -= i1), i1 );
            else if( ++i2 > 0 ) Array.Copy( x2, 0, x, (i -= i2), i2 );
            if( i == 0 ) return x;
            string[] xx = new string[x.Length - i];
            Array.Copy( x, i, xx, 0, xx.Length );
            return xx;
        }

        internal static string[] SortedArrayRemove( string[] x1, string[] x2, Comparison<string> comparer )
        {
            if( x1 == null ) throw new ArgumentNullException();
            if( x2 == null || x2.Length == 0 || x1.Length == 0 ) return x1;

            Debug.Assert( x1.IsSortedStrict( comparer ) && x2.IsSortedStrict( comparer ) );

            string[] x = (string[])x1.Clone();
            int minPos = x.Length;
            int i = minPos - 1, i2 = x2.Length - 1;
            string s = x[i];
            string s2 = x2[i2];
            int nbSlot = 0;
            for( ; ; )
            {
                int cmp = comparer( s, s2 );
                if( cmp < 0 )
                {
                    if( --i2 < 0 ) break;
                    s2 = x2[i2];
                }
                else if( cmp > 0 )
                {
                    if( --i < 0 ) break;
                    s = x[i];
                }
                else
                {
                    ++nbSlot;
                    minPos = i;
                    x[i] = _sMark;
                    --i;
                    --i2;
                    if( i < 0 || i2 < 0 ) break;
                    s = x[i];
                    s2 = x2[i2];
                }
            }
            if( nbSlot == 0 ) return x1;
            int len = x.Length - nbSlot;
            if( len == 0 ) return Util.EmptyStringArray;
            string[] xx = new string[len];
            Array.Copy( x, 0, xx, 0, minPos );
            Debug.Assert( ReferenceEquals( x[minPos], _sMark ) );
            i = minPos;
            --nbSlot;
            while( nbSlot != 0 )
            {
                s = x[++minPos];
                if( !ReferenceEquals( s, _sMark ) ) xx[i++] = s;
                else --nbSlot;
            }
            Array.Copy( x1, minPos + 1, xx, i, len - i );
            return xx;
        }

        internal static bool SortedArrayEquals( string[] x1, string[] x2, Comparison<string> comparer )
        {
            if( x1 == x2 ) return true;
            if( x1 == null || x2 == null || x1.Length != x2.Length ) return false;

            Debug.Assert( x1.IsSortedStrict( comparer ) );
            Debug.Assert( x2.IsSortedStrict( comparer ) );
            
            for( int i = 0; i < x1.Length; ++i ) if( x1[i] != x2[i] ) return false;
            return true;
        }

    }

}

