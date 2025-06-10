using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;

namespace UiStore.Services
{
    internal class AppAttack
    {
        private readonly CacheManager _cache;
        private readonly Logger _logger;
        private readonly InstanceWarehouse _instanceWarehouse;
        private CancellationTokenSource _ctsUpdate;
        private CancellationTokenSource _ctsExtact;

        private AppUnit AppUnit => _instanceWarehouse.AppUnit;
        private AppModel CurrentAppModel => AppModelManage.CurrentAppModel;
        private AppModelManagement AppModelManage => _instanceWarehouse.AppModelManagement;
        private AppStatusInfo AppStatus => _instanceWarehouse.AppStatusInfo;

        public AppAttack(CacheManager cache, InstanceWarehouse instanceWarehouse, Logger logger)
        {
            _cache = cache;
            _logger = logger;
            _instanceWarehouse = instanceWarehouse;
        }
        public void CancelUpdate()
        {
            _ctsUpdate?.Cancel();
        }

        public async Task CheckUpdate()
        {
            try
            {
                if (_ctsUpdate != null && !_ctsUpdate.Token.IsCancellationRequested) return;
                _ctsUpdate = new CancellationTokenSource();
                var appModel = await AppModelManage.GetAppModel();
                if (appModel == null)
                {
                    return;
                }
                if (!AppStatus.IsUpdateAble)
                {
                    return;
                }
                AppStatus.SetUpdating();
                try
                {
                    if (AppStatus.IsRunning && !_ctsUpdate.Token.IsCancellationRequested)
                    {
                        if (AppModelManage.IsModelChanged(appModel) || HasChangeProgramFiles(appModel))
                        {
                            AppStatus.HasNewVersion = true;
                        }
                    }
                    if (!await UpdateWareHouse(appModel, _ctsUpdate.Token))
                    {
                        AppStatus.SetUpdateFailed();
                    }
                    else
                    {
                        AppStatus.SetUpdateDone();
                    }
                }
                finally
                {
                    await UpdateIcon();
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine($"CheckUpdate:{ex.Message}");
            }
            finally
            {
                CancelUpdate();
            }

        }
        private async Task<bool> UpdateWareHouse(AppModel app, CancellationToken token)
        {
            int total = app.FileModels.Count;
            int done = 0;
            List<FileModel> checkedFiles = new List<FileModel>();
            List<FileModel> needToCheck = new List<FileModel>(app.FileModels);
            try
            {
                while (needToCheck.Count > 0 && !token.IsCancellationRequested)
                {
                    checkedFiles.Clear();
                    foreach (var fileModel in needToCheck)
                    {
                        if (await _cache.UpdateItem(fileModel))
                        {
                            AppStatus.Progress = (++done * 100) / total;
                            checkedFiles.Add(fileModel);
                        }
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    foreach (var fileModel in checkedFiles)
                    {
                        needToCheck.Remove(fileModel);
                    }
                }
                return done == total;
            }
            catch (Exception ex)
            {
                _logger.AddLogLine(ex.Message);
                return false;
            }
        }
        public void CancelExtack()
        {
            _ctsExtact?.Cancel();
        }

        private async Task<bool> ExtrackProgramFiles()
        {
            try
            {
                AppStatus.SetExtracting();
                if (_ctsExtact != null && !_ctsExtact.Token.IsCancellationRequested) return false;
                _ctsExtact = new CancellationTokenSource();
                int total = CurrentAppModel.FileModels.Count;
                int done = 0;
                List<FileModel> extrackedFiles = new List<FileModel>();
                List<FileModel> needToExtrack = new List<FileModel>(CurrentAppModel.FileModels);
                while (needToExtrack.Count > 0 && !_ctsExtact.Token.IsCancellationRequested)
                {
                    extrackedFiles.Clear();
                    foreach (var file in needToExtrack)
                    {
                        if (!IsCheckSumPass(file.Md5, file.StorePath) && !await _cache.ExtractFileTo(file.Md5, file.StorePath))
                        {
                            _logger.AddLogLine($"Extract failure! {file.ProgramPath}");
                            return false;
                        }
                        AppStatus.Progress = (++done * 100) / total;
                        extrackedFiles.Add(file);
                        if (_ctsExtact.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                    foreach (var fileModel in extrackedFiles)
                    {
                        needToExtrack.Remove(fileModel);
                    }
                }
                return done == total;
            }
            catch (Exception ex)
            {
                _logger.AddLogLine($"ExtrackProgramFiles:{ex.Message}");
                return false;
            }
            finally
            {
                CancelExtack();
            }
        }

        private async Task<(bool, string)> IconAppHasChanged(AppModel appModel)
        {
            var iconFile = appModel.AppIconFileModel;
            if (iconFile != null)
            {
                string iconPath = appModel.AppIconPath;
                return !IsCheckSumPass(iconFile.Md5, iconPath) && await _cache.ExtractFileTo(iconFile.Md5, iconPath) ? (true, iconPath) : (false, iconPath);
            }
            return (false, default);
        }

        private async Task UpdateIcon()
        {

            if (CurrentAppModel != null && !string.IsNullOrEmpty(CurrentAppModel.MainPath))
            {
                var rs = await IconAppHasChanged(CurrentAppModel);
                if (rs.Item1 && !string.IsNullOrEmpty(rs.Item2))
                {
                    AppUnit.ExtractIconFromApp(rs.Item2);
                }
            }
        }

        private static bool IsCheckSumPass(string md5, string storePath)
        {
            if (File.Exists(storePath))
            {
                string fileMd5 = Util.GetMD5HashFromFile(storePath);
                if (fileMd5 == md5)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        private bool HasChangeProgramFiles(AppModel app)
        {
            int total = app.FileModels.Count;
            int done = 0;
            bool result = false;
            foreach (var file in app.FileModels)
            {
                if (!IsCheckSumPass(file.Md5, file.StorePath))
                {
                    result = true;
                }
                AppStatus.Progress = (++done * 100) / total;
            }
            return result;
        }

        private readonly object _lock = new object();
        internal async Task Open()
        {
            try
            {
                if (!AppStatus.IsRunnable || CurrentAppModel?.FileModels == null) return;
                lock (_lock)
                {
                    if (!AppStatus.IsRunnable || CurrentAppModel?.FileModels == null) return;
                }
                AppStatus.IsRunning = true;
                AppStatus.SetExtracting();
                if (await ExtrackProgramFiles())
                {
                    AppStatus.SetExtractDone();
                    string cmd = $"cd \"{_instanceWarehouse.AppInfoModel.ProgramFolderPath}\" && {CurrentAppModel.OpenCmd}";
                    Util.RunCmd(cmd);
                }
                else
                {
                    AppStatus.SetExtractFailed();
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine($"Open:{ex.Message}");
            }
            finally
            {
                AppStatus.IsRunning = false;
            }
        }
    }
}
