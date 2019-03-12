using System;
using System.IO.Compression;
using System.Threading;
using NLog;

namespace GZipArchiver
{
    internal abstract class Worker
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly bool logging;

        public event EventHandler<WorkerErrorEventArgs> OnWorkerError;

        public bool Runable { get; set; }

        protected CompressionMode Mode { get; }

        public Worker(CompressionMode mode, bool logging)
        {
            this.logging = logging;
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

        protected void LogginInfo(string message)
        {
            if (logging)
                logger.Info(message);
        }

        protected void LoggingInfo(string message, object[] args)
        {
            if (logging)
                logger.Info(message, args);
        }

        protected void LoggingError(Exception exc, string message)
        {
            if (logging)
                logger.Error(exc, message);
        }

        protected void LoggingError(Exception exc, string message, object[] args)
        {
            if (logging)
                logger.Error(exc, message, args);
        }

    }
}
