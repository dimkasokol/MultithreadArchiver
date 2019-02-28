using System;
using System.IO.Compression;
using System.Threading;

namespace GZipArchiver
{
    internal abstract class Worker
    {
        public event EventHandler<WorkerErrorEventArgs> OnWorkerError;

        public bool Runable { get; set; }

        protected CompressionMode Mode { get; }

        public Worker(CompressionMode mode)
        {
            Runable = true;
            Mode = mode;
        }

        public virtual Thread GetWorkerThread()
        {
            switch (Mode)
            {
                case CompressionMode.Compress:
                    return new Thread(CompressionWorker);
                case CompressionMode.Decompress:
                    return new Thread(DecompressionWorker);
                default:
                    throw new Exception("unknown compression mode");
            }
        }

        protected abstract void CompressionWorker();

        protected abstract void DecompressionWorker();

        protected void InvokeWorkerError(Exception exc) => OnWorkerError?.Invoke(this, new WorkerErrorEventArgs(exc));
    }
}
