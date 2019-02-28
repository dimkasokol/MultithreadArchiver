namespace GZipArchiver
{
    internal class BytesBlock
    {
        public int Id { get; }

        public int Length => Bytes.Length;

        public byte[] Bytes { get; }

        public BytesBlock()
        {
            Id = -1;
            Bytes = new byte[0];
        }

        public BytesBlock(int id, byte[] bytes)
        {
            Id = id;
            Bytes = bytes;
        }
    }
}
