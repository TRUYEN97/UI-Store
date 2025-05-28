using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace UiStore.Common
{
    internal class ZipHelper
    {
        public static async Task ExtractSingleFileFromStream(Stream zipStream, string localPath, string zipPassword = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));
            await Task.Run(() =>
            {
                using (var zipInput = new ZipInputStream(zipStream))
                {
                    if (!string.IsNullOrWhiteSpace(zipPassword))
                    {
                        zipInput.Password = zipPassword;
                    }
                    ZipEntry entry = zipInput.GetNextEntry() ?? throw new InvalidOperationException("File ZIP rỗng.");
                    if (entry.IsDirectory)
                        throw new InvalidOperationException("File đầu tiên trong ZIP là thư mục.");
                    using (var outputStream = File.Create(localPath))
                    {
                        zipInput.CopyTo(outputStream);
                    }
                }
            });
        }

        public static async Task ZipSingleFiletoStream(string entryName, Stream zipStream, string localPath, string zipPassword = null)
        {
            await Task.Run(() =>
            {
                using (var zipOutputStream = new ZipOutputStream(zipStream))
                {
                    zipOutputStream.SetLevel(9); // Mức nén tối đa
                    if (!string.IsNullOrWhiteSpace(zipPassword))
                    {
                        zipOutputStream.Password = zipPassword;
                    }
                    var entry = new ZipEntry(entryName)
                    {
                        DateTime = File.GetLastWriteTime(localPath)
                    };
                    zipOutputStream.PutNextEntry(entry);
                    using (var fileStream = File.OpenRead(localPath))
                    {
                        fileStream.CopyTo(zipOutputStream);
                    }
                    zipOutputStream.CloseEntry();
                    zipOutputStream.IsStreamOwner = false;
                    zipOutputStream.Close();
                }
            });
        }

        public static void ExtractZipWithPassword(string zipFilePath, string extractDirectory, string password)
        {
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException("Không tìm thấy file ZIP.", zipFilePath);

            Directory.CreateDirectory(extractDirectory);

            using (var fs = File.OpenRead(zipFilePath))
            using (var zipStream = new ZipInputStream(fs))
            {
                if (!string.IsNullOrWhiteSpace(password))
                {
                    zipStream.Password = password;
                }
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    if (string.IsNullOrWhiteSpace(entry.Name))
                        continue;

                    string fullPath = Path.Combine(extractDirectory, entry.Name);

                    // Chống path traversal
                    if (!fullPath.StartsWith(Path.GetFullPath(extractDirectory), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Đường dẫn không hợp lệ trong ZIP.");

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(fullPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                    using (var outputStream = File.Create(fullPath))
                    {
                        zipStream.CopyTo(outputStream);
                    }
                }
            }
        }

        public static async Task ExtractSingleFileWithPassword(string zipFilePath, string targetPath, string password = null)
        {
            await Task.Run(() =>
            {
                if (!File.Exists(zipFilePath))
                    throw new FileNotFoundException(zipFilePath);
                using (var fs = File.OpenRead(zipFilePath))
                using (var zipStream = new ZipInputStream(fs))
                {
                    if (!string.IsNullOrWhiteSpace(password))
                    { 
                        zipStream.Password = password; 
                    }
                    ZipEntry entry;
                    if((entry = zipStream.GetNextEntry()) != null)
                    {
                        if (entry.IsDirectory)
                        {
                            throw new Exception($"Zip file invailed: {targetPath}");
                        }
                        string dir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrWhiteSpace(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        using (var outputStream = File.Create(targetPath))
                        {
                            zipStream.CopyTo(outputStream);
                        }
                    }
                }
            });
        }

        public static bool ExtractSingleFileWithPassword_LongPath(
    string zipFilePath,
    string outputFilePath,
    string password = null,
    string outputFileNameOverride = null)
        {
            try
            {
                if (!File.Exists(zipFilePath))
                    return false;

                // Đảm bảo đường dẫn tuyệt đối
                zipFilePath = Path.GetFullPath(zipFilePath);
                outputFilePath = Path.GetFullPath(outputFilePath);

                // Xử lý long path
                if (!zipFilePath.StartsWith(@"\\?\"))
                    zipFilePath = @"\\?\" + zipFilePath;
                if (!outputFilePath.StartsWith(@"\\?\"))
                    outputFilePath = @"\\?\" + outputFilePath;

                using (var fs = File.OpenRead(zipFilePath))
                using (var zipInputStream = new ZipInputStream(fs))
                {
                    if (!string.IsNullOrWhiteSpace(password))
                        zipInputStream.Password = password;

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (entry.IsDirectory) continue;

                        string fileName = outputFileNameOverride ?? Path.GetFileName(entry.Name);
                        string fullOutputPath = Path.Combine(Path.GetDirectoryName(outputFilePath), fileName);

                        if (!fullOutputPath.StartsWith(@"\\?\"))
                            fullOutputPath = @"\\?\" + fullOutputPath;

                        string dir = Path.GetDirectoryName(fullOutputPath);
                        if (!string.IsNullOrEmpty(dir))
                            Directory.CreateDirectory(dir);

                        using (var outputStream = File.Create(fullOutputPath))
                        {
                            zipInputStream.CopyTo(outputStream);
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi giải nén: " + ex.Message);
                return false;
            }
        }
    }
}
