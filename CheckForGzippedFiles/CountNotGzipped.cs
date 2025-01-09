using System;
using System.IO;

namespace CountNotGzipped
{
    class CountNotGzipped
    {
        static void Main(string[] args)
        {
            var path = "/var/lib/docker/volumes/container_name/_data";

            Console.WriteLine($"Is \"{path}\" the right directory? \n y/N");

            var choice = Console.ReadLine();
            if (choice != "y" || !Directory.Exists(path))
            {
                if (choice == "y")
                {
                    Console.WriteLine($"Specified directory is not valid!");
                }

                path = string.Empty;
                while (string.IsNullOrWhiteSpace(path))
                {
                    path = ChoseAnotherPath();
                }
            }

            Console.WriteLine($"Specified directory is valid");

            if (!AskForConfirm()) return;
            var count = CountCompressed(path);
            Console.WriteLine(
                $"{count.Gzipped} Files compressed! \n {count.NotGzipped} Files not compressed \n {count.ZeroSize} Files of 0 size");
        }

        private static string ChoseAnotherPath()
        {
            Console.WriteLine("Enter directory path:");
            var chosenPath = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(chosenPath))
            {
                if (Directory.Exists(chosenPath))
                {
                    return chosenPath;
                }
            }

            Console.WriteLine("Directory is not valid.");
            return string.Empty;
        }

        private static bool AskForConfirm()
        {
            Console.WriteLine("\n Do you want to count not compressed files in this directory? \n y/N");
            var result = Console.ReadLine();
            return result == "y";
        }

        private static CountResult CountCompressed(string path)
        {
            var directory = new DirectoryInfo(path);
            var gzipped = 0;
            var notGzipped = 0;
            var zeroSize = 0;
            foreach (var file in directory.GetFiles())
            {
                var fileBytes = File.ReadAllBytes(file.FullName);
                if (fileBytes.Length != 0)
                {
                    if (!FileGzipped(fileBytes))
                    {
                        notGzipped++;
                    }
                    else
                    {
                        gzipped++;
                    }
                }
                else
                {
                    zeroSize++;
                    Console.WriteLine($"{file.Name} Size is 0. Skipping");
                }
            }

            return new CountResult()
            {
                NotGzipped = notGzipped,
                Gzipped = gzipped,
                ZeroSize = zeroSize
            };
        }

        private static bool FileGzipped(byte[] fileBytes)
        {
            // GZIP file format states the first 2 bytes of the file are '\x1F' and '\x8B'
            return (fileBytes[0] == 0x1f) && (fileBytes[1] == 0x8b);
        }
    }

    class CountResult
    {
        public int Gzipped { get; set; }

        public int NotGzipped { get; set; }

        public int ZeroSize { get; set; }
    }
}