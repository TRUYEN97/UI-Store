using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Xml.Linq;
using UiStore.Common;
using UiStore.Model;
using static UiStore.Common.ConstKey;

namespace UiStore.Services
{
    internal class AppUpdater
    {
        private readonly CacheManager _cache;

        public event Action<string> OnLog;
        public event Action<int> OnProgress;
        public event Action<int> OnStatus;
        public string ProgramFolderPath;
        public string CommonFolderPath;
        public string Name { get; set; }


        public AppUpdater(CacheManager cache)
        {
            _cache = cache;
        }

        public async Task UpdateAsync(AppModel app)
        {
            bool rs = true;
            try
            {
                OnLog?.Invoke($"[{Name}] đang check cập nhật");
                int total = app.FileModels.Count;
                int done = 0;
                OnStatus.Invoke(AppState.UPDATE_STATE);
                foreach (var file in app.FileModels)
                {
                    string zipName = Path.GetFileName(file.RemotePath);
                    string zipPath = Path.Combine(CommonFolderPath, zipName);
                    if (!await CheckUpdate(file, zipPath))
                    {
                        rs = false;
                    }
                    done++;
                    OnProgress?.Invoke((done * 100) / total);
                }
            }
            finally
            {
                OnStatus.Invoke(AppState.STANDBY_STATE);
                if (rs)
                {
                    OnLog?.Invoke($"[{Name}] Cập nhật hoàn tất");
                }
                else
                {
                    OnLog?.Invoke($"[{Name}] Cập nhật thất bại!");
                }
            }
        }

        public async Task<bool> CreateProgram( AppModel app)
        {
            bool rs = true;
            try
            {
                int total = app.FileModels.Count;
                int done = 0;
                OnStatus.Invoke(AppState.CREATE_STATE);
                foreach (var file in app.FileModels)
                {
                    if (!(await CheckSumAppFiles(file)).Item1)
                    {
                        rs = false;
                    }
                    done++;
                    OnProgress?.Invoke((done * 100) / total);
                }
                OnStatus.Invoke(AppState.STANDBY_STATE);
                return rs;
            }
            finally
            {
                if (rs)
                {
                    OnLog?.Invoke($"[{Name}] Khởi tạo thành công");
                }
                else
                {
                    OnLog?.Invoke($"[{Name}] Khởi tạo thất bại!");
                }
            }
        }

        private async Task<bool> CheckUpdate(FileModel file, string storeFile)
        {
            if (_cache.TryGetPathByMd5(file.Md5, out _))
            {
                return true;
            }
            using (var sftp = Util.GetSftpInstance())
            {
                if (await sftp.DownloadFile(file.RemotePath, storeFile))
                {
                    _cache.Add(file.Md5, storeFile);
                    return true;
                }
                return false;
            }
        }


        public async Task<(bool, string)> CheckSumAppFiles(FileModel file)
        {
            return await Task.Run(async () =>
            {
                if (file == null || string.IsNullOrWhiteSpace(ProgramFolderPath))
                {
                    return (false, default);
                }
                string storeFile = Path.Combine(ProgramFolderPath, file.ProgramPath);
                if (IsCheckSumPass(file, storeFile))
                {
                    return (true, storeFile);
                }
                if (Util.IsFileLocked(storeFile))
                {
                    OnLog.Invoke($"[{Name}] - [{storeFile}] đang mở. Cập nhật thất bại!");
                    return (false, default);
                }
                return !await ExtractFileFromCache(file, storeFile) ? (false, default) : (true, storeFile);
            });
        }

        private async Task<bool> ExtractFileFromCache(FileModel file, string storeFile)
        {
            if (_cache.TryGetPathByMd5(file.Md5, out var cachedPath))
            {
                try
                {
                    await ZipHelper.ExtractSingleFileWithPassword(cachedPath, storeFile, ZIP_PASSWORD);
                    if (IsCheckSumPass(file, storeFile))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke($"[{Name}] Lỗi khi Unzip file {cachedPath} -> {storeFile}: {ex.Message}");
                }
                _cache.TryRemove(file.Md5);
            }
            return false;
        }

        private static bool IsCheckSumPass(FileModel file, string storeFile)
        {
            return File.Exists(storeFile) && Util.GetMD5HashFromFile(storeFile).Equals(file.Md5);
        }
    }
}
