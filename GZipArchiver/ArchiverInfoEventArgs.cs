using System;
using System.IO.Compression;

namespace GZipArchiver
{
    public class ArchiverInfoEventArgs : EventArgs
    {
        public CompressionMode Mode { get; }

        public long FileLength { get; }

        public long Position { get; }

        public ArchiverInfoEventArgs(CompressionMode mode, long length, long position)
        {
            Mode = mode;
            FileLength = length;
            Position = position;
        }
    }
}
