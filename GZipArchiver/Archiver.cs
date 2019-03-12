using System;
using System.Collections.Generic;
using System.IO.Compression;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace GZipArchiver
{
    public class Archiver
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly bool logging;
        private bool inProcessing;
        private ArchiverException archiverException;
        private List<Worker> workers;

        public event EventHandler<ArchiverInfoEventArgs> StatusUpdated;

        public Archiver(bool logging)
        {
            inProcessing = false;
            this.logging = logging;
            if (logging)
                ConfigureLogger();
        }

        public void Compress(string input, string output) => Processing(input, output, CompressionMode.Compress);

        public void Decompress(string input, string output) => Processing(input, output, CompressionMode.Decompress);

        private void Processing(string input, string output, CompressionMode mode)
        {
            if (inProcessing)
                throw new ArchiverException("compressing already in progress", null);
            inProcessing = true;

            if (logging)
                logger.Info("{0}ion {1} to {2} started", new object[] { mode, input, output });

            archiverException = null;
            workers = new List<Worker>(3);

            var reader = new Reader(mode, input, logging);
            reader.OnWorkerError += WorkerErrorReceived;
            reader.GetWorkerThread().Start();
            workers.Add(reader);

            var compressor = new Compressor(mode, reader, logging);
            compressor.OnWorkerError += WorkerErrorReceived;
            for (var i = 0; i < Environment.ProcessorCount; i++)
                compressor.GetWorkerThread().Start();
            workers.Add(compressor);

            var writer = new Writer(mode, output, compressor, logging);
            writer.OnWorkerError += WorkerErrorReceived;
            writer.GetWorkerThread().Start();
            workers.Add(writer);

            while (true)
            {
                if (archiverException != null)
                {
                    foreach (var worker in workers)
                        worker.Runable = false;
                    if (logging)
                        logger.Error(archiverException, "{0}ion {1} to {2} failed", new object[] { mode, input, output });
                    throw archiverException;
                }

                StatusUpdated?.Invoke(this, new ArchiverInfoEventArgs(mode, reader.FileLength, reader.Position));

                if (!workers.Exists(w => w.Runable))
                    break;
            }

            if (logging)
                logger.Info("{0}ion {1} to {2} finished", new object[] { mode, input, output });
            LogManager.Shutdown();
        }

        private void WorkerErrorReceived(object sender, WorkerErrorEventArgs e) => 
            archiverException = new ArchiverException($"Error source: {sender}\r\nCouse: {e.Exception.Message}", e.Exception);

        private void ConfigureLogger()
        {
            var config = new LoggingConfiguration();
            config.AddRule(LogLevel.Info, LogLevel.Fatal, new FileTarget("logfile") { FileName = "log.txt" });
            LogManager.Configuration = config;
        }
    }
}
