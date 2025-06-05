using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public HashSet<FileModel> FilesToRemove { get; internal set; }

        public AppUpdater(CacheManager cache, AppUnit appUnit, Logger logger)
        {
            _cache = cache;
            _appUnit = appUnit;
            _logger = logger;
        }

        public async Task CheckUpdate(AppModel app)
        {
            if (app == null || _appUnit.DoStatus != DoStatus.DO_NOTHING)
            {
                return;
            }
            try
            {
                _appUnit.DoStatus = DoStatus.CHECK_UPDATE_STATE;
                _cache.RegisterLink(app);
                if (_appUnit.IsRunning)
                {
                    if (HasChangeProgramFiles(app))
                    {
                        _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> the program has a new version!");
                        _appUnit.AppStatus = AppStatus.HAS_NEW_VERSION;
                    }
                }
                if (!await _cache.UpdateWareHouse(app, _appUnit))
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> update the warehouse failure!");
                    _appUnit.AppStatus = AppStatus.UPDATE_FAILED;
                }
                else
                {
                    if (!_appUnit.IsRunning && _appUnit.AppStatus == AppStatus.HAS_NEW_VERSION)
                    {
                        _appUnit.AppStatus = AppStatus.STANDBY;
                    }
                }
            }
            finally
            {
                await UpdateIcon(app);
                _appUnit.DoStatus = DoStatus.DO_NOTHING;
            }
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
                    if (!IsCheckSumPass(file.Md5, file.StorePath) && !await _cache.ExtractFileTo(file.Md5, file.StorePath, _appUnit))
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
                    if (_appUnit.AppStatus == AppStatus.HAS_NEW_VERSION)
                    {
                        _appUnit.AppStatus = AppStatus.STANDBY;
                    }
                }
                else
                {
                    _logger.AddLogLine($"[{_appUnit.AppInfoModel.Name}] -> Launch failure!");
                    _currentAppModel = null;
                }
                _appUnit.DoStatus = DoStatus.DO_NOTHING;
            }
        }

        private async Task<(bool, string)> IconAppHasChanged(AppModel appModel)
        {
            var iconFile = appModel.FileModels.FirstOrDefault(f => Util.ArePathsEqual(f.ProgramPath, appModel.MainPath));
            if (iconFile != null)
            {
                string iconPath = Path.Combine(_appUnit.AppInfoModel.IconDir, iconFile.ProgramPath);
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
            if (HasChangeFileModels(app))
            {
                return true;
            }
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
            if (!result)
            {
                _currentAppModel = app;
            }
            return result;
        }

        private bool HasChangeFileModels(AppModel app)
        {
            if (_currentAppModel?.FileModels != null)
            {
                var toRemoves = new HashSet<FileModel>(_currentAppModel.FileModels);
                toRemoves.ExceptWith(app.FileModels);
                if (toRemoves.Count > 0)
                {
                    _cache.TryRemove(toRemoves);
                    FilesToRemove = toRemoves;
                    return true;
                }
                FilesToRemove?.Clear();
                return false;
            }
            FilesToRemove?.Clear();
            return true;
        }

    }
}
