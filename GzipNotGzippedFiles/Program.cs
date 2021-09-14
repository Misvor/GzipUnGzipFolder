using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace GzipNotGzippedFiles
{
	class Program
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

			if (choice == "I want to decompress!")
			{
				Console.WriteLine("Your wish is granted");
				DecompressFilesInFolder(path);
				Console.WriteLine("Files decompressed!");
			}

			if (AskForConfirm())
			{
				CompressFilesInFolder(path);
				Console.WriteLine("Files compressed!");
			}
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
			Console.WriteLine("\n Are you sure you want to COMPRESS all files in this directory? \n y/N");
			var result = Console.ReadLine();
			return result == "y";
		}

		private static void CompressFilesInFolder(string path)
		{
			var directory = new DirectoryInfo(path);
			if (directory.Exists)
			{
				foreach (var file in directory.GetFiles())
				{
					var fileBytes = File.ReadAllBytes(file.FullName);
					if (fileBytes.Length != 0)
					{
						if (!FileGzipped(fileBytes))
						{
							var fileData = file.OpenRead();
							byte[] zippedBytes = Compress(fileData);
							fileData.Dispose();
							if (zippedBytes.Length != 0)
							{
								var compressedFile = File.Create(file.FullName);
								compressedFile.Write(zippedBytes, 0, zippedBytes.Length);
							}
							else
							{
								Console.WriteLine($"could not compress {file.Name} Skipping");
							}
						}
					}
					else
					{
						Console.WriteLine($"{file.Name} is corrupted! Skipping");
					}

				}
			}
		}

		private static byte[] Compress(Stream data)
		{
			using (var compressedStream = new MemoryStream())
			{
				using (var zipStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
				{
					data.CopyTo(zipStream);
					zipStream.Close();
					var result = compressedStream.ToArray();
					compressedStream.Dispose();
					return result;
				}
			}
		}

		private static bool FileGzipped(byte[] fileBytes)
		{
			// GZIP file format states the first 2 bytes of the file are '\x1F' and '\x8B'
			return (fileBytes[0] == 0x1f) && (fileBytes[1] == 0x8b);

		}

		#region Decompress
		private static void DecompressFilesInFolder(string path)
		{
			var directory = new DirectoryInfo(path);
			if (directory.Exists)
			{
				foreach (var file in directory.GetFiles())
				{
					var fileBytes = File.ReadAllBytes(file.FullName);
					if (FileGzipped(fileBytes))
					{
						var originalBytes = Decompress(fileBytes);
						var originalFile = File.Create(file.FullName);
						originalFile.Write(originalBytes, 0, originalBytes.Length);
					}
				}
			}
		}

		private static byte[] Decompress(byte[] data)
		{
			using (var compressedStream = new MemoryStream(data))
			{
				using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
				{
					var resultStream = new MemoryStream();

					zipStream.CopyTo(resultStream);
					resultStream.Seek(0, SeekOrigin.Begin);
					var result = resultStream.ToArray();
					resultStream.Dispose();
					if (result.Length != 0)
					{
						return result;
					}
					else
					{
						return data;
					}
				}
			}
		}
		#endregion

	}
}
