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

        public void RegisterLink(AppModel appModel)
        {
            if (appModel?.FileModels == null)
            {
                return;
            }
            foreach (var file in appModel?.FileModels)
            {
                if(TryGetPathByMd5(file.Md5, out var cachedItem))
                {
                    cachedItem.AddLink(file.StorePath);
                }
            }
        }


        public async Task<bool> ExtractFileTo(string md5, string storePath, AppUnit appUnit)
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
                }
                catch (Exception ex)
                {
                    _logger.AddLogLine($"[{appUnit.AppInfoModel.Name}] Unzip.Extract, {cachedItem} -> {storePath}: {ex.Message}");
                    return false;
                }
                finally
                {
                    cachedItem.Status = CacheItem.DlStatus.DoNothing;
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
                    _logger.AddLogLine($"[{appUnit.AppInfoModel.Name}] Unzip.checksum, {storePath}: {ex.Message}");
                    return false;
                }
                Remove(md5);
            }
            return false;
        }

        public async Task<bool> UpdateWareHouse(AppModel app, AppUnit appUnit)
        {
            int total = app.FileModels.Count;
            int done = 0;
            List<FileModel> toRemote = new List<FileModel>();
            List<FileModel> NeedToCheck = new List<FileModel>(app.FileModels);
            while (NeedToCheck.Count > 0)
            {
                toRemote.Clear();
                foreach (var fileModel in NeedToCheck)
                {
                    var cacheItem = await UpdateItem(fileModel, appUnit);
                    if (cacheItem.Standby)
                    {
                        appUnit.SetProgress((++done * 100) / total);
                        toRemote.Add(fileModel);
                    }
                    else if (cacheItem.Init)
                    {
                        break;
                    }
                }
                foreach (var fileModel in toRemote)
                {
                    NeedToCheck.Remove(fileModel);
                }
            }
            return done == total;
        }

        private async Task<CacheItem> UpdateItem(FileModel fileModel, AppUnit appUnit)
        {
            var cacheItem = GetCacheItem(fileModel, appUnit);
            if (cacheItem.Standby)
            {
                return cacheItem;
            }
            cacheItem = await Download(fileModel);
            return cacheItem;
        }

        private CacheItem GetCacheItem(FileModel fileModel, AppUnit appUnit)
        {
            if (!_cache.ContainsKey(fileModel.Md5))
            {
                lock (_createLock)
                {
                    if (!_cache.ContainsKey(fileModel.Md5))
                    {
                        string zipPath = Path.Combine(appUnit.AppInfoModel.CommonFolderPath, Path.GetFileName(fileModel.RemotePath));
                        _cache[fileModel.Md5] = new CacheItem(zipPath);
                    }
                }
            }
            return _cache[fileModel.Md5];
        }

        private async Task<CacheItem> Download(FileModel fileModel)
        {
            try
            {
                if (_cache.TryGetValue(fileModel.Md5, out var cacheItem) && cacheItem.Init)
                {
                    try
                    {
                        lock (_updateLock)
                        {
                            if (!cacheItem.Init)
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

        internal void TryRemove(HashSet<FileModel> toRemoves)
        {
            if (toRemoves == null)
            {
                return;
            }
            foreach (var fileModel in toRemoves)
            {
                TryRemove(fileModel);
            }
        }

        internal void TryRemove(FileModel toRemove)
        {
            if (_cache.TryGetValue(toRemove.Md5, out var cacheItem))
            {
                cacheItem.RemoveLink(toRemove.StorePath);
                if (cacheItem.IsUseless())
                {
                    Remove(toRemove.Md5);
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

            public bool Init => Status == DlStatus.Init;

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
