using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;
using static UiStore.Common.ConstKey;

namespace UiStore.Services
{
    internal class CacheManager
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache;
        private readonly Logger _logger;
        private readonly object _createLock;
        private readonly object _updateLock;

        public CacheManager(Logger logger)
        {
            _cache = new ConcurrentDictionary<string, CacheItem>();
            _logger = logger;
            _createLock = new object();
            _updateLock = new object();
        }

        public void LoadFromFolder(string baseFolder)
        {
            if (Directory.Exists(baseFolder))
            {
                foreach (var file in Directory.EnumerateFiles(baseFolder, "*.zip", SearchOption.AllDirectories))
                {
                    string md5 = Path.GetFileNameWithoutExtension(file);
                    _cache[md5] = new CacheItem(file)
                    {
                        Status = CacheItem.DlStatus.DoNothing
                    };
                }
            }
        }


        public async Task<bool> ExtractFileTo(string md5, string storePath)
        {
            if (string.IsNullOrWhiteSpace(md5))
            {
                return false;
            }
            if (TryGetPathByMd5(md5, out var cachedItem) && cachedItem.Standby)
            {
                try
                {
                    cachedItem.Status = CacheItem.DlStatus.Extracting;
                    await ZipHelper.ExtractSingleFileWithPassword(cachedItem.Path, storePath, ZIP_PASSWORD);
                    cachedItem.Status = CacheItem.DlStatus.DoNothing;
                }
                catch (Exception ex)
                {
                    _logger.AddLogLine($"Unzip.Extract, {cachedItem} -> {storePath}: {ex.Message}");
                    cachedItem.Status = CacheItem.DlStatus.Init;
                    return false;
                }
                try
                {
                    if (File.Exists(storePath) && Util.GetMD5HashFromFile(storePath) == md5)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.AddLogLine($"Unzip.checksum, {storePath}: {ex.Message}");
                    cachedItem.Status = CacheItem.DlStatus.Init;
                    return false;
                }
                Remove(md5);
            }
            return false;
        }

        public async Task<bool> UpdateItem(FileModel fileModel, string appName)
        {
            var cacheItem = GetCacheItem(fileModel);
            cacheItem.AddLink(appName);
            if (cacheItem.Standby)
            {
                return true;
            }
            cacheItem = await Download(fileModel);
            return cacheItem.Standby;
        }

        private CacheItem GetCacheItem(FileModel fileModel)
        {
            if (!_cache.ContainsKey(fileModel.Md5))
            {
                lock (_createLock)
                {
                    if (!_cache.ContainsKey(fileModel.Md5))
                    {
                        var cacheItem = new CacheItem(fileModel.ZipPath);
                        _cache[fileModel.Md5] = cacheItem;
                    }
                }
            }
            return _cache[fileModel.Md5];
        }

        private async Task<CacheItem> Download(FileModel fileModel)
        {
            try
            {
                if (_cache.TryGetValue(fileModel.Md5, out var cacheItem) && cacheItem.CanUpdate)
                {
                    try
                    {
                        lock (_updateLock)
                        {
                            if (!cacheItem.CanUpdate)
                            {
                                return cacheItem;
                            }
                            cacheItem.Status = CacheItem.DlStatus.Loading;
                        }
                        using (var sftp = Util.GetSftpInstance())
                        {
                            if (await sftp.DownloadFile(fileModel.RemotePath, cacheItem.Path))
                            {
                                cacheItem.Status = CacheItem.DlStatus.DoNothing;
                            }
                            else
                            {
                                cacheItem.Status = CacheItem.DlStatus.Init;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.AddLogLine(ex.Message);
                        cacheItem.Status = CacheItem.DlStatus.Init;
                    }
                }
                return cacheItem;
            }
            catch (Exception ex)
            {
                _logger.AddLogLine(ex.Message);
                return default;
            }
        }

        private bool TryGetPathByMd5(string md5, out CacheItem itemInfo)
        {
            try
            {
                if (_cache.TryGetValue(md5, out itemInfo))
                {
                    if (itemInfo.Exists())
                    {
                        return true;
                    }
                    _cache.TryRemove(md5, out _);
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine(ex.Message);
            }
            itemInfo = default;
            return false;
        }

        private void Remove(string md5)
        {
            if (_cache.TryGetValue(md5, out var cacheItem) && cacheItem.Standby)
            {
                _cache.TryRemove(md5, out _);
                if (File.Exists(cacheItem.Path))
                {
                    File.Delete(cacheItem.Path);
                }
            }
        }

        internal void TryRemoveWith(string appName)
        {
            if (string.IsNullOrEmpty(appName))
            {
                return;
            }
            var toRemoveMd5 = new List<string>();
            foreach (var cacheItem in _cache)
            {
                cacheItem.Value.RemoveLink(appName);
                if (cacheItem.Value.IsUseless())
                {
                    toRemoveMd5.Add(cacheItem.Key);
                }
            }
            foreach (var md5 in toRemoveMd5)
            {
                _cache.TryRemove(md5, out var cacheItem);
                if (_cache.TryRemove(md5, out _) && File.Exists(cacheItem.Path))
                {
                    File.Delete(cacheItem.Path);
                }
            }
        }

        private class CacheItem
        {
            public enum DlStatus { Extracting, Loading, DoNothing, Init }
            private readonly HashSet<string> linked;
            public CacheItem(string path)
            {
                linked = new HashSet<string>();
                Path = path;
                Status = DlStatus.Init;
            }
            public string Path { get; private set; }
            public DlStatus Status { get; set; }
            public bool Standby => Status == DlStatus.DoNothing && Exists();

            public bool CanUpdate => Status == DlStatus.Init || (Status == DlStatus.DoNothing && !Exists());
            public void RemoveLink(string link)
            {
                linked.Remove(link);
            }

            public void AddLink(string link)
            {
                linked.Add(link);
            }

            public bool IsUseless()
            {
                return linked.Count == 0 || !Exists();
            }

            internal bool Exists()
            {
                return !string.IsNullOrWhiteSpace(Path) && File.Exists(Path);
            }
        }
    }
}
