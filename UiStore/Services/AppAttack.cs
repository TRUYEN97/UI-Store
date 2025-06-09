using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

        public async Task CheckUpdate()
        {
            try
            {
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
                    if (AppStatus.IsRunning)
                    {
                        if (AppModelManage.IsModelChanged(appModel) || HasChangeProgramFiles(appModel))
                        {
                            AppStatus.HasNewVersion = true;
                        }
                    }
                    if (!await UpdateWareHouse(appModel))
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
                _logger.AddLogLine(ex.Message);
            }

        }
        private async Task<bool> UpdateWareHouse(AppModel app)
        {
            int total = app.FileModels.Count;
            int done = 0;
            List<FileModel> checkedFiles = new List<FileModel>();
            List<FileModel> needToCheck = new List<FileModel>(app.FileModels);
            try
            {
                while (needToCheck.Count > 0)
                {
                    checkedFiles.Clear();
                    foreach (var fileModel in needToCheck)
                    {
                        if (await _cache.UpdateItem(fileModel))
                        {
                            AppStatus.Progress = (++done * 100) / total;
                            checkedFiles.Add(fileModel);
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

        public async Task<bool> CreateProgram()
        {
            if (!AppStatus.IsExtractable || CurrentAppModel?.FileModels == null)
            {
                return false;
            }
            try
            {
                AppStatus.SetExtracting();
                int total = CurrentAppModel.FileModels.Count;
                int done = 0;
                foreach (var file in CurrentAppModel.FileModels)
                {
                    if (!IsCheckSumPass(file.Md5, file.StorePath) && !await _cache.ExtractFileTo(file.Md5, file.StorePath))
                    {
                        _logger.AddLogLine($"Extract failure! {file.ProgramPath}");
                        AppStatus.SetExtractFailed();
                        return false;
                    }
                    AppStatus.Progress = (++done * 100) / total;
                }
                AppStatus.SetExtractDone();
                AppModelManage.UpdateUseModel();
                return true;
            }
            catch (Exception ex)
            {
                _logger.AddLogLine(ex.Message);
                AppStatus.SetExtractFailed();
                return false;
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
            return File.Exists(storePath) && Util.GetMD5HashFromFile(storePath).Equals(md5);
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

        internal async Task Open()
        {
            try
            {
                AppStatus.IsRunning = true;
                if (await CreateProgram())
                {
                    string cmd = $"cd \"{_instanceWarehouse.AppInfoModel.ProgramFolderPath}\" && {CurrentAppModel.OpenCmd}";
                    Util.RunCmd(cmd);
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine(ex.Message);
            }
            finally
            {
                AppStatus.IsRunning = false;
            }
        }
    }
}
