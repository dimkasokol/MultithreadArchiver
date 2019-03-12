using System;
using GZipArchiver;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[]
            //{
            //    "compress",
            //    "book.pdf",
            //    "_book.pdf",
            //    "logging"
            //};
            args = new string[]
            {
                "decompress",
                "_book.pdf.gz",
                "_book.pdf"
            };
#endif
            if (args.Length < 3 || args.Length > 4)
            {
                WriteResult("wrong number of parameters");
                return;
            }

            var logging = false;
            if (args.Length == 4)
            {
                if (args[3] != "logging")
                {
                    WriteResult($"wrong parameter {args[3]}");
                    return;
                }
                logging = true;
            }

            var archiver = new Archiver(logging);
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
