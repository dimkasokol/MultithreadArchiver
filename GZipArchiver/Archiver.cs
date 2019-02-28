﻿using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace GZipArchiver
{
    public class Archiver
    {
        private ArchiverException archiverException;
        private List<Worker> workers;

        public event EventHandler<ArchiverInfoEventArgs> StatusUpdated;

        public void Compress(string input, string output) => Processing(input, output, CompressionMode.Compress);

        public void Decompress(string input, string output) => Processing(input, output, CompressionMode.Decompress);

        private void Processing(string input, string output, CompressionMode mode)
        {
            archiverException = null;
            workers = new List<Worker>(3);

            var reader = new Reader(mode, input);
            reader.OnWorkerError += WorkerErrorReceived;
            reader.GetWorkerThread().Start();
            workers.Add(reader);

            var compressor = new Compressor(mode, reader);
            compressor.OnWorkerError += WorkerErrorReceived;
            for (var i = 0; i < Environment.ProcessorCount; i++)
                compressor.GetWorkerThread().Start();
            workers.Add(compressor);

            var writer = new Writer(mode, output, compressor);
            writer.OnWorkerError += WorkerErrorReceived;
            writer.GetWorkerThread().Start();
            workers.Add(writer);

            while (true)
            {
                if (archiverException != null)
                {
                    reader.Runable = false;
                    throw archiverException;
                }
                if (!workers.Exists(w => w.Runable))
                    return;
                StatusUpdated?.Invoke(this, new ArchiverInfoEventArgs(mode, reader.FileLength, reader.Position));
            }
        }

        private void WorkerErrorReceived(object sender, WorkerErrorEventArgs e) => 
            archiverException = new ArchiverException($"Error source: {sender}\r\nCouse: {e.Exception.Message}", e.Exception);
    }
}