using System;
using GZipArchiver;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[3]
            //{
            //    "compress",
            //    "book.pdf",
            //    "_book.pdf"
            //};
            args = new string[3]
            {
                "decompress",
                "_book.pdf.gz",
                "_book.pdf"
            };
#endif
            if (args.Length != 3)
            {
                WriteResult("wrong number of parameters");
                return;
            }

            var archiver = new Archiver();
            archiver.StatusUpdated += WriteProgress;

            try
            {
                switch (args[0])
                {
                    case "compress":
                        archiver.Compress(args[1], args[2]);
                        WriteResult($"{args[1]} compressed to {args[2]} successfuly");
                        return;
                    case "decompress":
                        archiver.Decompress(args[1], args[2]);
                        WriteResult($"{args[1]} decompressed to {args[2]} successfuly");
                        return;
                    default:
                        throw new Exception("incorrect command");
                }
            }
            catch (Exception exc)
            {
                WriteResult(exc);
            }
        }

        private static void WriteResult(string message)
        {
            Console.WriteLine($"\r\n{message}");
            Console.ReadKey();
        }

        private static void WriteResult(Exception exc) => WriteResult($"{exc.Message}");

        private static void WriteProgress(object sender, ArchiverInfoEventArgs e) => Console.Write($"\r{e.Mode}: processed {e.Position / 1024} KB of {e.FileLength / 1024} KB");
    }
}
