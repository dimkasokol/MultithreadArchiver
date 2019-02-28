using System.IO.Compression;
using System.Threading;

namespace GZipArchiver
{
    internal abstract class FileWorker : Worker
    {
        protected string FileName { get; }
        private Thread thread;

        public FileWorker(CompressionMode mode, string file) : base(mode)
        {
            FileName = file;
        }

        public override Thread GetWorkerThread()
        {
            if (thread == null)
                thread = base.GetWorkerThread();
            return thread;
        }
    }
}
