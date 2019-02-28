using System;

namespace GZipArchiver
{
    internal class WorkerErrorEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public WorkerErrorEventArgs(Exception exc)
        {
            Exception = exc;
        }
    }
}
