using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipArchiver
{
    internal class Compressor : Worker, IProcessedHolder
    {
        private readonly IReadedHolder readedHolder;
        private readonly Dictionary<int, BytesBlock> blockDictionary;
        private readonly object dictionaryLocker;

        public Compressor(CompressionMode mode, IReadedHolder readedHolder, bool logging) : base(mode, logging)
        {
            this.readedHolder = readedHolder;
            blockDictionary = new Dictionary<int, BytesBlock>();
            dictionaryLocker = new object();
        }

        public BytesBlock GetBytesBlock(int id)
        {
            lock(dictionaryLocker)
            {
                var block = new BytesBlock();

                while (!blockDictionary.TryGetValue(id, out block))
                    if (Runable)
                        Monitor.Wait(dictionaryLocker);
                    else
                        return null;

                blockDictionary.Remove(id);
                Monitor.PulseAll(dictionaryLocker);
                return block;
            }
        }

        private void AddBytesBlock(BytesBlock block)
        {
            lock (dictionaryLocker)
            {
                while (blockDictionary.Count > 64)
                    Monitor.Wait(dictionaryLocker);

                blockDictionary.Add(block.Id, block);

                Monitor.PulseAll(dictionaryLocker);
            }
        }

        protected override void CompressionWorker()
        {
            try
            {
                while (Runable)
                {
                    var block = readedHolder.GetBytesBlock();
                    if (block == null)
                        return;

                    LoggingInfo("Compressing block {0}", new object[] { block.Id });

                    byte[] bytes;
                    using (var memStream = new MemoryStream())
                    {
                        using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                            gzipStream.Write(block.Bytes, 0, block.Length);
                        bytes = memStream.ToArray();
                    }
                    AddBytesBlock(new BytesBlock(block.Id, bytes));
                }
            }
            catch (Exception exc)
            {
                LoggingError(exc, "Compressing failed");
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
                while (Runable)
                {
                    var compressedBlock = readedHolder.GetBytesBlock();
                    if (compressedBlock == null)
                        return;

                    LoggingInfo("Decompressing block {0}", new object[] { compressedBlock.Id });

                    BytesBlock originalBlock;
                    using (var memStream = new MemoryStream(compressedBlock.Bytes))
                    {
                        var bytes = new byte[BitConverter.ToInt32(compressedBlock.Bytes, compressedBlock.Length - 4)];

                        using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                            gzipStream.Read(bytes, 0, bytes.Length);

                        originalBlock = new BytesBlock(compressedBlock.Id, bytes);
                    }
                    AddBytesBlock(originalBlock);
                }
            }
            catch (Exception exc)
            {
                LoggingError(exc, "Decompressing failed");
                InvokeWorkerError(exc);
            }
            finally
            {
                Runable = false;
            }
        }
    }
}
