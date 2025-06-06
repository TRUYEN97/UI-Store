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
    internal class AppUpdater
    {
        private readonly CacheManager _cache;
        private readonly AppUnit _appUnit;
        private readonly AppModelManagement _appModelManagement;

        public HashSet<FileModel> FilesToRemove { get; internal set; }

        public AppModel CurrentAppModel => _appModelManagement.CurrentAppModel;
        private Logger Logger => _appUnit.Logger;
        private AppStatusInfo AppStatus => _appUnit.AppStatusInfo;

        public AppUpdater(CacheManager cache, AppUnit appUnit)
        {
            _cache = cache;
            _appUnit = appUnit;
            _appModelManagement = new AppModelManagement(appUnit);
        }

        public async Task CheckUpdate()
        {
            if ( !AppStatus.IsUpdateAble || !await _appModelManagement.UpdateAppModel())
            {
                return;
            }
            var appModel = _appModelManagement.CurrentAppModel;
            try
            {
                AppStatus.SetUpdating();
                if (AppStatus.IsRunning)
                {
                    if (HasChangeProgramFiles(appModel))
                    {
                        Logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> the program has a new version!");
                        AppStatus.HasNewVersion = true;
                    }
                    else
                    {
                        AppStatus.HasNewVersion = false;
                    }
                }
                if (!await _cache.UpdateWareHouse(appModel, _appUnit))
                {
                    Logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> update the warehouse failure!");
                    AppStatus.SetUpdateFailed();
                }
                else
                {
                    if (!AppStatus.IsRunning)
                    {
                        AppStatus.HasNewVersion = false;
                    }
                    AppStatus.SetUpdateDone();
                }
            }
            finally
            {
                await UpdateIcon(appModel);
            }
        }

        public async Task<bool> CreateProgram(AppModel app)
        {
            if (!AppStatus.IsExtractable)
            {
                return false;
            }
            try
            {
                AppStatus.SetExtracting();
                int total = app.FileModels.Count;
                int done = 0;
                foreach (var file in app.FileModels)
                {
                    if (!IsCheckSumPass(file.Md5, file.StorePath) && !await _cache.ExtractFileTo(file.Md5, file.StorePath, _appUnit))
                    {
                        Logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> Launch failure! {file.ProgramPath}");
                        AppStatus.SetExtractFailed();
                        return false;
                    }
                    done++;
                    _appUnit.SetProgress((done * 100) / total);
                }
                Logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> Launch success!");
                AppStatus.SetExtractDone();
                return true;
            }
            catch (Exception ex)
            {
                Logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> {ex.Message}");
                Logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> Launch failure!");
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
                return !IsCheckSumPass(iconFile.Md5, iconPath) && await _cache.ExtractFileTo(iconFile.Md5, iconPath, _appUnit) ? (true, iconPath) : (false, iconPath);
            }
            return (false, default);
        }

        private async Task UpdateIcon(AppModel appModel)
        {
            if (!string.IsNullOrEmpty(appModel.MainPath))
            {
                var rs = await IconAppHasChanged(appModel);
                if (rs.Item1 && !string.IsNullOrEmpty(rs.Item2))
                {
                    _appUnit.ExtractIconFromApp(rs.Item2);
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
                done++;
                _appUnit.SetProgress((done * 100) / total);
            }
            return result;
        }
    }
}
