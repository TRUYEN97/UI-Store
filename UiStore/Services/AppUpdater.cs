using System;
using System.IO;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;
using static UiStore.Common.ConstKey;

namespace UiStore.Services
{
    internal class AppUpdater
    {
        private readonly CacheManager _cache;
        private readonly AppUnit _appUnit;
        private readonly Logger _logger;
        private AppModel _currentAppModel;

        public AppUpdater(CacheManager cache, AppUnit appUnit, Logger logger)
        {
            _cache = cache;
            _appUnit = appUnit;
            _logger = logger;
        }

        public async Task CheckUpdate(AppModel app)
        {
            if (_appUnit.DoStatus != DoStatus.DO_NOTHING)
            {
                return;
            }
            try
            {
                _appUnit.DoStatus = DoStatus.CHECK_UPDATE_STATE;
                if (_appUnit.IsRunning)
                {
                    if (!CheckUpdateProgramFiles(app))
                    {
                        _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> the program has a new version!");
                        _appUnit.AppStatus = AppStatus.HAS_NEW_VERSION;
                    }
                }
                if (!await UpdateWareHouse(app))
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> update the warehouse failure!");
                    _appUnit.AppStatus = AppStatus.UPDATE_FAILED;
                }
                else
                {
                    if (!_appUnit.IsRunning)
                    {
                        _appUnit.AppStatus = AppStatus.STANDBY;
                    }
                }
            }
            finally
            {
                _appUnit.DoStatus = DoStatus.DO_NOTHING;
            }
        }

        private bool CheckUpdateProgramFiles(AppModel app)
        {
            bool result = true;
            int total = app.FileModels.Count;
            int done = 0;
            if (_currentAppModel?.FileModels == null || !_currentAppModel.FileModels.SetEquals(app.FileModels))
            {
                result = false;
            }
            foreach (var file in app.FileModels)
            {
                if (HasChanged(file).Item1)
                {
                    result = false;
                }
                done++;
                _appUnit.SetProgress((done * 100) / total);
            }
            return result;
        }

        private async Task<bool> UpdateWareHouse(AppModel app)
        {
            bool result = true;
            int total = app.FileModels.Count;
            int done = 0;
            foreach (var file in app.FileModels)
            {
                if (!await UpdateWareHouseFile(file))
                {
                    result = false;
                }
                done++;
                _appUnit.SetProgress((done * 100) / total);
            }
            return result;
        }

        public async Task<bool> CreateProgram(AppModel app)
        {
            if (_appUnit.DoStatus != DoStatus.DO_NOTHING)
            {
                return false;
            }
            bool rs = true;
            try
            {
                _appUnit.DoStatus = DoStatus.CREATE_STATE;
                int total = app.FileModels.Count;
                int done = 0;
                foreach (var file in app.FileModels)
                {
                    if (!await PrepareFile(file))
                    {
                        rs = false;
                    }
                    done++;
                    _appUnit.SetProgress((done * 100) / total);
                }
                return rs;
            }
            finally
            {
                if (rs)
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> Launch success!");
                    _currentAppModel = app;
                }
                else
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> Launch failure!");
                    _currentAppModel = null;
                }
                _appUnit.DoStatus = DoStatus.DO_NOTHING;
            }
        }

        private async Task<bool> UpdateWareHouseFile(FileModel file)
        {
            string zipName = Path.GetFileName(file.RemotePath);
            string zipPath = Path.Combine(_appUnit.AppInfoModel.CommonFolderPath, zipName);
            if (_cache.TryGetPathByMd5(file.Md5, out _))
            {
                return true;
            }
            using (var sftp = Util.GetSftpInstance())
            {
                if (await sftp.DownloadFile(file.RemotePath, zipPath))
                {
                    _cache.Add(file.Md5, zipPath);
                    return true;
                }
                return false;
            }
        }

        private async Task<bool> PrepareFile(FileModel file)
        {
            return await Task.Run(async () =>
            {
                if (file == null || string.IsNullOrWhiteSpace(_appUnit.AppInfoModel.ProgramFolderPath))
                {
                    return false;
                }
                var rs = HasChanged(file);
                string storeFile = rs.Item2;
                if (!rs.Item1)
                {
                    return true;
                }
                if (Util.IsFileLocked(storeFile))
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] - [{storeFile}] file is locked or in use!");
                    return false;
                }
                return await ExtractFileFromCache(file, storeFile);
            });
        }

        public async Task<(bool, string)> IconAppHasChanged(FileModel file)
        {
            return await Task.Run(async () =>
            {
                if (file == null || string.IsNullOrWhiteSpace(_appUnit.AppInfoModel.ProgramFolderPath))
                {
                    return (false, default);
                }
                var rs = HasChanged(file);
                string storeFile = rs.Item2;
                if (!rs.Item1)
                {
                    return (false, storeFile);
                }
                if (Util.IsFileLocked(storeFile))
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] - [{storeFile}] file is locked or in use!");
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
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] Lỗi khi Unzip file {cachedPath} -> {storeFile}: {ex.Message}");
                }
                _cache.TryRemove(file.Md5);
            }
            return false;
        }

        private (bool, string) HasChanged(FileModel file)
        {
            string storeFile = Path.Combine(_appUnit.AppInfoModel.ProgramFolderPath, file.ProgramPath);
            return (!IsCheckSumPass(file, storeFile), storeFile);
        }

        private static bool IsCheckSumPass(FileModel file, string storeFile)
        {
            return File.Exists(storeFile) && Util.GetMD5HashFromFile(storeFile).Equals(file.Md5);
        }
    }
}
