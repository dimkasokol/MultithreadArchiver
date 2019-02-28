using System;

namespace GZipArchiver
{
    public class ArchiverException : Exception
    {
        public ArchiverException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
