using System;
using System.IO;
using System.IO.Compression;

namespace GZipArchiver
{
    internal class Writer : FileWorker
    {
        private readonly IProcessedHolder processedHolder;

        public Writer(CompressionMode mode, string file, IProcessedHolder processedHolder, bool logging) : base(mode, file, logging)
        {
            this.processedHolder = processedHolder;
        }

        protected override void CompressionWorker()
        {
            try
            {
                var fileName = $"{FileName}.gz";

                if (File.Exists(fileName))
                    File.Delete(fileName);

                var blockId = 0;

                while (Runable)
                {
                    var block = processedHolder.GetBytesBlock(blockId);
                    if (block == null)
                        return;

                    LoggingInfo("Writing block {0}", new object[] { block.Id });

                    BitConverter.GetBytes(block.Length).CopyTo(block.Bytes, 4);

                    using (var stream = File.OpenWrite(fileName))
                    {
                        stream.Position = stream.Length;
                        stream.Write(block.Bytes, 0, block.Length);
                    }

                    blockId++;
                }
            }
            catch (Exception exc)
            {
                LoggingError(exc, "File writing failed");
                InvokeWorkerError(exc);
            }
            finally
            {
                Runable = false;
            }
        }

        protected override void DecompressionWorker()
        {
            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);

                var blockId = 0;

                while (Runable)
                {
                    var block = processedHolder.GetBytesBlock(blockId);
                    if (block == null)
                        return;

                    LoggingInfo("Writing block {0}", new object[] { block.Id });

                    using (var stream = File.OpenWrite(FileName))
                    {
                        stream.Position = stream.Length;
                        stream.Write(block.Bytes, 0, block.Length);
                    }

                    blockId++;
                }
            }
            catch (Exception exc)
            {
                LoggingError(exc, "File writing failed");
                InvokeWorkerError(exc);
            }
            finally
            {
                Runable = false;
            }
        }
    }
}
