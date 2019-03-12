using System.IO.Compression;
using System.Threading;

namespace GZipArchiver
{
    internal abstract class FileWorker : Worker
    {
        protected string FileName { get; }
        private Thread thread;

        public FileWorker(CompressionMode mode, string file, bool logging) : base(mode, logging)
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
