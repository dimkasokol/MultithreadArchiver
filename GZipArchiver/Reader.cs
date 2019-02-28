using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipArchiver
{
    internal class Reader : FileWorker, IReadedHolder
    {
        private readonly Queue<BytesBlock> blockQueue;
        private readonly object queueLocker;

        public long FileLength { get; private set; }

        public long Position { get; private set; }

        public Reader(CompressionMode mode, string input) : base(mode, input)
        {
            blockQueue = new Queue<BytesBlock>();
            queueLocker = new object();
        }

        public BytesBlock GetBytesBlock()
        {
            lock (queueLocker)
            {
                while (blockQueue.Count == 0 && Runable)
                    Monitor.Wait(queueLocker);

                if (blockQueue.Count == 0)
                    return null;

                var block = blockQueue.Dequeue();
                Monitor.PulseAll(queueLocker);
                return block;
            }
        }

        private void AddBytesBlock(BytesBlock block)
        {
            lock (queueLocker)
            {
                while (blockQueue.Count > 512)
                    Monitor.Wait(queueLocker);

                blockQueue.Enqueue(block);

                Monitor.PulseAll(queueLocker);
            }
        }

        protected override void CompressionWorker()
        {
            try
            {
                using (var stream = File.OpenRead(FileName))
                {
                    FileLength = stream.Length;
                    var blockLength = 1024 * 1024;
                    var blockId = 0;

                    while (stream.Position < stream.Length)
                    {
                        if (!Runable)
                            return;

                        Position = stream.Position;

                        var bytesExpect = stream.Length - stream.Position;
                        var bytesToRead = blockLength < bytesExpect ? blockLength : (int)bytesExpect;
                        var bytes = new byte[bytesToRead];
                        stream.Read(bytes, 0, bytesToRead);

                        AddBytesBlock(new BytesBlock(blockId, bytes));
                        blockId++;
                    }
                }
            }
            catch (Exception exc)
            {
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
                using (var stream = File.OpenRead(FileName))
                {
                    FileLength = stream.Length;
                    var blockId = 0;

                    while (stream.Position < stream.Length)
                    {
                        var lengthBlock = new byte[8];
                        stream.Read(lengthBlock, 0, lengthBlock.Length);
                        var blockLength = BitConverter.ToInt32(lengthBlock, 4);

                        var bytesBlock = new byte[blockLength];
                        lengthBlock.CopyTo(bytesBlock, 0);
                        stream.Read(bytesBlock, 8, blockLength - 8);

                        AddBytesBlock(new BytesBlock(blockId, bytesBlock));
                        blockId++;
                        Position = stream.Position;
                    }
                }
            }
            catch (Exception exc)
            {
                InvokeWorkerError(exc);
            }
            finally
            {
                Runable = false;
            }
        }
    }
}
