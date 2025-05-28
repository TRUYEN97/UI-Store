using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UiStore.Configs;
using UiStore.Services;

namespace UiStore.Common
{
    public class Util
    {

        public static async Task<bool> CopyFile(string cachedPath, string storeFile)
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(storeFile));
                File.Copy(cachedPath, storeFile, true);
            });
            return true;
        }

        public static bool ArePathsEqual(string path1, string path2)
        {
            string normalizedPath1 = Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar);
            string normalizedPath2 = Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar);

            return string.Equals(normalizedPath1, normalizedPath2, StringComparison.OrdinalIgnoreCase);
        }
        public static string RunCmdWithConsole(string command, bool isWaitForExit = true)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                if (isWaitForExit)
                {
                    process.WaitForExit();
                }
                return string.IsNullOrEmpty(error) ? output : error;
            }
        }
        public static string RunCmd(string command, bool isWaitForExit = true)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                if (isWaitForExit)
                {
                    process.WaitForExit();
                }
                return string.IsNullOrEmpty(error) ? output : error;
            }
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                if (filePath == null || !File.Exists(filePath))
                {
                    return false;
                }
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        internal static string GetMD5HashFromFile(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hash)
                        sb.Append(b.ToString("x2"));
                    return sb.ToString();
                }
            }
        }

        internal static MySftp GetSftpInstance()
        {
            var _configModel = AutoDLConfig.ConfigModel;
            return new MySftp(
                _configModel.SftpConfig.Host,
                _configModel.SftpConfig.Port,
                _configModel.SftpConfig.User,
                _configModel.SftpConfig.Password);
        }

        internal static void OpenFile(string storePath)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = storePath,
                UseShellExecute = true
            });
        }

        internal static string GetMD5HashFromString(string input)
        {
            if (input == null)
            {
                return null;
            }
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); 
                }
                return sb.ToString();
            }
        }

        internal static string GetAppName(Location location, string name)
        {
            return $"{location.Product}-{location.Station}-{name}";
        }
    }
}
