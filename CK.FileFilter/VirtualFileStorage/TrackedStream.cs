#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CK.FileFilter
{
    internal class TrackedStream : Stream
    {
        Stream _s;
        VirtualFileStorage _fs;

        public TrackedStream( VirtualFileStorage fs, Stream s )
        {
            _fs = fs;
            ++_fs._countOpenStream;
            _s = s;
        }

        public override bool CanRead
        {
            get { return _s.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _s.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _s.CanWrite; }
        }

        public override void Flush()
        {
            _s.Flush();
        }

        public override long Length
        {
            get { return _s.Length; }
        }

        public override long Position
        {
            get
            {
                return _s.Position;
            }
            set
            {
                _s.Position = value;
            }
        }

        public override int Read( byte[] buffer, int offset, int count )
        {
            return _s.Read( buffer, offset, count );
        }

        public override long Seek( long offset, SeekOrigin origin )
        {
            return _s.Seek( offset, origin );
        }

        public override void SetLength( long value )
        {
            _s.SetLength( value );
        }

        public override void Write( byte[] buffer, int offset, int count )
        {
            _s.Write( buffer, offset, count );
        }

        public override void Close()
        {
            if( _fs != null )
            {
                --_fs._countOpenStream;
                _fs = null;
            }
            base.Close();
        }
    }
}
#endif