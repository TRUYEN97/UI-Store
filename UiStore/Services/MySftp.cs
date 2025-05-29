using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using UiStore.Common;

namespace UiStore.Services
{
    public class MySftp : IDisposable
    {
        private readonly SftpClient _client;
        public MySftp(string host, int port, string user, string password)
        {
            _client = new SftpClient(host, port, user, password);
            try
            {
                _client.Connect();
            }
            catch (Exception)
            {
            }
        }

        public async Task<bool> Connect()
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        return true;
                    }
                    else
                    {
                        _client.Connect();
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }


        public bool IsConnected { get { return _client.IsConnected; } }

        public async Task<string> ReadAllText(string remotePath)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    if (!await Connect())
                    {
                        return null;
                    }
                    if (!_client.Exists(remotePath))
                    {
                        return null;
                    }
                    return _client.ReadAllText(remotePath);
                });
            }
            catch (Exception)
            {
                return null;
            }

        }
        public async Task<string[]> ReadAllLines(string remotePath)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    if (!await Connect())
                    {
                        return null;
                    }

                    if (!_client.Exists(remotePath))
                    {
                        return null;
                    }
                    return _client.ReadAllLines(remotePath);
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> WriteAllText(string remotePath, string contents)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    if (!await Connect())
                    {
                        return false;
                    }
                    if (!await CreateDirectory(Path.GetDirectoryName(remotePath)))
                    {
                        return false;
                    }
                    _client.WriteAllText(remotePath, contents);
                    return true;
                });
            }
            catch (Exception)
            {
                return false;
            }

        }
        public async Task<bool> DownloadFile(string remotePath, string localPath)
        {
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (!await Connect())
                    {
                        return false;
                    }
                    if (!_client.Exists(remotePath))
                    {
                        return false;
                    }
                    string dir = Path.GetDirectoryName(localPath);
                    Directory.CreateDirectory(dir);
                    await Task.Run(() =>
                    {
                        using (var fileStream = new FileStream(localPath, FileMode.Create))
                        {
                            _client.DownloadFile(remotePath, fileStream);
                        }
                    });
                    return true;
                }
                catch (Exception)
                {
                    if (attempt == maxRetries)
                        return false;
                    await Task.Delay(10);
                }
            }
            return false;
        }
        public async Task<bool> DownloadFileAndUnzip(string remotePath, string localPath, string zipPassword = null)
        {
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (await Connect())
                    {
                        if (!_client.Exists(remotePath))
                        {
                            return false;
                        }
                        return await Task.Run(async () =>
                        {
                            using (var zipStream = new MemoryStream())
                            {
                                _client.DownloadFile(remotePath, zipStream);
                                zipStream.Position = 0;
                                await ZipHelper.ExtractSingleFileFromStream(zipStream, localPath, zipPassword);
                                return true;
                            }
                        });
                    }
                }
                catch (Exception)
                {
                    if (attempt == maxRetries)
                        return false;
                    await Task.Delay(10);
                }
            }
            return false;
        }

        internal async Task<string> DownloadZipFileFormModel(string remotePath, string zipPassword)
        {
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (await Connect())
                    {
                        if (!_client.Exists(remotePath))
                        {
                            return null;
                        }
                        return await Task.Run(async () =>
                        {
                            using (var zipStream = new MemoryStream())
                            {
                                _client.DownloadFile(remotePath, zipStream);
                                zipStream.Position = 0;
                                return await ZipHelper.ExtractToJsonString(zipStream, zipPassword);
                            }
                        });
                    }
                }
                catch (Exception)
                {
                    if (attempt == maxRetries)
                        return null;
                    await Task.Delay(10);
                }
            }
            return null;
        }

        public async Task<List<string>> DownloadFolder(string remotePath, string localPath, bool isDowndloadAll = false)
        {
            try
            {
                if (!await Connect())
                {
                    return null;
                }
                var fileNames = new List<string>();
                if (_client.Exists(remotePath))
                {
                    foreach (var file in _client.ListDirectory(remotePath))
                    {
                        if (isDowndloadAll && file.IsDirectory && !file.Name.StartsWith("."))
                        {
                            string subFolder = string.Format("{0}/{1}", localPath, file.Name);
                            fileNames.AddRange(await DownloadFolder(file.FullName, subFolder, isDowndloadAll));
                        }
                        else if (!file.IsDirectory)
                        {
                            string subPath = file.FullName.Substring(remotePath.Length);
                            string filePath = string.Format("{0}{1}", localPath, subPath);
                            Directory.CreateDirectory(localPath);
                            await Task.Run(() =>
                            {
                                using (var fileStreem = new FileStream(filePath, FileMode.Create))
                                {
                                    _client.DownloadFile(file.FullName, fileStreem);
                                    fileNames.Add(filePath);
                                }
                            });
                        }
                    }
                }
                return fileNames;
            }
            catch (Exception)
            {
                return null;
            }

        }
        public async Task<bool> UploadZipFile(string remotePath, string localPath, string zipPassword)
        {
            try
            {
                if (!File.Exists(localPath))
                {
                    return false;
                }
                using (var zipStream = new MemoryStream())
                {
                    await ZipHelper.ZipSingleFiletoStream(Path.GetFileNameWithoutExtension(remotePath), zipStream, localPath, zipPassword);
                    int maxRetries = 3;
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        try
                        {
                            zipStream.Position = 0;
                            if (await Connect())
                            {
                                if (await CreateDirectory(Path.GetDirectoryName(remotePath)))
                                {
                                    _client.UploadFile(zipStream, remotePath);
                                    return true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (attempt == maxRetries)
                                return false;
                            await Task.Delay(10);
                        }
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async Task<bool> UploadZipFileFormModel(object model, string remotePath, string zipPassword)
        {
            try
            {
                string json = JsonConvert.SerializeObject(model, Formatting.Indented);
                using (var zipStream = new MemoryStream())
                {
                    await ZipHelper.JsonAsZipToStream(zipStream, json, Path.GetFileNameWithoutExtension(remotePath), zipPassword);
                    int maxRetries = 3;
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        try
                        {
                            zipStream.Position = 0;
                            if (await Connect())
                            {
                                if (await CreateDirectory(Path.GetDirectoryName(remotePath)))
                                {
                                    _client.UploadFile(zipStream, remotePath);
                                    return true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            if (attempt == maxRetries)
                                return false;
                            await Task.Delay(10);
                        }
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UploadFile(string remotePath, string localPath)
        {
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {

                if (!File.Exists(localPath))
                {
                    return false;
                }
                return await Task.Run(async () =>
                {
                    try
                    {
                        if (!await Connect())
                        {
                            return false;
                        }
                        if (!await CreateDirectory(Path.GetDirectoryName(remotePath)))
                        {
                            return false;
                        }
                        using (var fileStreem = new FileStream(localPath, FileMode.Open))
                        {
                            _client.UploadFile(fileStreem, remotePath);
                        }
                        return true;
                    }
                    catch (Exception)
                    {
                        if (attempt == maxRetries)
                            return false;
                        await Task.Delay(10);
                    }
                    return false;
                });
            }
            return false;

        }
        public async Task<bool> DeleteFile(string remotePath)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (!await Connect())
                    {
                        return false;
                    }
                    if (_client.Exists(remotePath))
                    {
                        _client.DeleteFile(remotePath);
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }
        public async Task<bool> CreateDirectory(string remotePath)
        {

            return await Task.Run(async () =>
            {
                try
                {
                    string[] parts = remotePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    string currentPath = "";

                    if (remotePath.StartsWith("/"))
                    {
                        currentPath = "/";
                    }
                    else if (remotePath.StartsWith("\\"))
                    {
                        currentPath = "\\";
                    }
                    if (!await Connect())
                    {
                        return false;
                    }
                    foreach (var part in parts)
                    {
                        currentPath = Path.Combine(currentPath, part).Replace('\\', '/');

                        if (!_client.Exists(currentPath))
                        {
                            _client.CreateDirectory(currentPath);
                        }
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });

        }
        public async Task<List<string>> ListDirectoryPath(string remotePath)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (!await Connect())
                    {
                        return null;
                    }
                    List<string> listName = new List<string>();
                    if (!_client.Exists(remotePath))
                    {
                        return null;
                    }
                    foreach (var file in _client.ListDirectory(remotePath))
                    {
                        if (file.IsDirectory && !file.Name.StartsWith("."))
                        {
                            listName.Add(file.FullName);
                        }
                    }
                    return listName;
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }
        public async Task<List<string>> ListDirectoryName(string remotePath)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (!await Connect())
                    {
                        return null;
                    }
                    List<string> listName = new List<string>();
                    if (!_client.Exists(remotePath))
                    {
                        return null;
                    }
                    foreach (var file in _client.ListDirectory(remotePath))
                    {
                        if (file.IsDirectory && !file.Name.StartsWith("."))
                        {
                            listName.Add(file.Name);
                        }
                    }
                    return listName;
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }
        public async Task<List<string>> ListFilePath(string remotePath, bool getAll)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    if (!await Connect())
                    {
                        return null;
                    }
                    List<string> listName = new List<string>();
                    if (!_client.Exists(remotePath))
                    {
                        return null;
                    }
                    foreach (var file in _client.ListDirectory(remotePath))
                    {
                        if (file.IsDirectory && getAll)
                        {
                            var temp = await ListFilePath(remotePath, getAll);
                            if (temp != null)
                            {
                                listName.AddRange(temp);
                            }
                        }
                        else
                        {
                            listName.Add(file.FullName);
                        }
                    }
                    return listName;
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public async Task<bool> DeleteFolder(string remotePath, bool deleteIfEmpty = false)
        {

            if (!await Connect())
            {
                return false;
            }
            return await Task.Run(async () =>
            {
                try
                {
                    if (_client.Exists(remotePath))
                    {
                        List<ISftpFile> list = new List<ISftpFile>(_client.ListDirectory(remotePath));
                        if (list.Count > 0 && !deleteIfEmpty)
                        {
                            foreach (var file in list)
                            {
                                if (!file.Name.StartsWith("."))
                                {
                                    if (file.IsDirectory)
                                    {
                                        await DeleteFolder(file.FullName);
                                    }
                                    else
                                    {
                                        _client.DeleteFile(file.FullName);
                                    }
                                }
                            }
                        }
                        _client.DeleteDirectory(remotePath);
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }


        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task<bool> Exists(string remotePath)
        {
            if (!await Connect())
            {
                return false;
            }
            return _client.Exists(remotePath);
        }
    }
}
