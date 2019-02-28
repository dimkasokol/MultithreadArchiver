namespace GZipArchiver
{
    internal interface IProcessedHolder
    {
        BytesBlock GetBytesBlock(int id);
    }
}
