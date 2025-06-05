using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Models;
using UiStore.ViewModel;

namespace UiStore.Services
{
    internal class ProgramManagement
    {
        public ObservableCollection<AppViewModel> Applications { get; } = new ObservableCollection<AppViewModel>();
        private readonly Dictionary<string, AppViewModel> _appBackgrounds = new Dictionary<string, AppViewModel>();
        private readonly CacheManager _cache;
        private readonly Logger _logger;
        public ProgramManagement(CacheManager cache, Logger logger)
        {
            _cache = cache;
            _logger = logger;
        }
        public void UpdateApps(Dictionary<string, ProgramPathModel> newConfigs)
        {
            CheckUpdateApps(newConfigs);
            RemoveAppsNotExists(newConfigs);
        }

        public void AddApp(AppViewModel appViewModel)
        {
            if (!IsContainApp(appViewModel))
            {
                DispatcherHelper.RunOnUI(() =>
                {
                    Applications.Add(appViewModel);
                    _logger.AddLogLine($"Add [{appViewModel?.AppInfoModel?.Name}]");
                });
            }
        }

        public void RemoveApp(AppViewModel appViewModel, AppModel appModel)
        {
            DisableApp(appViewModel);
            if (_appBackgrounds.ContainsKey(appViewModel.Name))
            {
                _appBackgrounds.Remove(appViewModel.Name);
                appViewModel.Dispose();
                _logger.AddLogLine($"Remove [{appViewModel?.AppInfoModel?.Name}]");
                if (appModel?.CloseAndClear == true)
                {
                    RemoveProgramFolder(appViewModel.AppInfoModel);
                }
                _cache.TryRemove(appModel?.FileModels);
            }
        }

        public void RemoveProgramFolder(AppInfoModel appInfoModel)
        {
            try
            {
                string dir = appInfoModel?.ProgramFolderPath;
                if (!string.IsNullOrEmpty(dir))
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
                RemoveAppIcon(appInfoModel);
            }
            catch (Exception ex)
            {
                _logger.AddLogLine($"[{appInfoModel.Name}]: {ex.Message}");
            }
        }

        internal void RemoveAppIcon(AppInfoModel appInfoModel)
        {
            try
            {
                string dir = appInfoModel?.IconDir;
                if (!string.IsNullOrEmpty(dir))
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine($"[{appInfoModel.Name}]: {ex.Message}");
            }
        }

        internal void RemoveProgram(HashSet<FileModel> filesToRemove)
        {
            if (filesToRemove != null)
            {
                try
                {
                    foreach (var fileModel in filesToRemove)
                    {
                        if (File.Exists(fileModel.StorePath))
                        {
                            File.Delete(fileModel.StorePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.AddLogLine(ex.Message);
                }
            }
        }

        public void DisableApp(AppViewModel appViewModel)
        {
            if (!Applications.Contains(appViewModel))
            {
                return;
            }
            DispatcherHelper.RunOnUI(() =>
            {
                Applications.Remove(appViewModel);
            });
            _logger.AddLogLine($"Disable [{appViewModel?.AppInfoModel?.Name}]");
        }

        private bool TryGetBackGroupApp(string name, out AppViewModel app)
        {
            if (!string.IsNullOrEmpty(name) && _appBackgrounds.TryGetValue(name, out app))
            {
                return true;
            }
            app = null;
            return false;
        }

        private void AddAppToBackGroud(AppViewModel app)
        {
            string name = app?.AppInfoModel?.Name;
            if (!string.IsNullOrEmpty(name) && !TryGetBackGroupApp(name, out _))
            {
                _appBackgrounds.Add(name, app);
            }
        }

        private bool IsContainApp(AppViewModel appViewModel)
        {
            foreach (var app in Applications)
            {
                if (app == appViewModel || app?.AppInfoModel?.AppPath == appViewModel?.AppInfoModel?.AppPath)
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckUpdateApps(Dictionary<string, ProgramPathModel> newConfigs)
        {
            foreach (var config in newConfigs)
            {
                if (!TryGetBackGroupApp(config.Key, out var appVm))
                {
                    appVm = new AppViewModel(_cache, this, config.Value, _logger);
                    appVm.AppInfoModel.Name = config.Key;
                    appVm.AppInfoModel.RootDir = AutoDLConfig.ConfigModel.AppLocalPath;
                    appVm.AppInfoModel.CommonFolderPath = AutoDLConfig.ConfigModel.CommonLocalPath;
                    appVm.StartUpdate();
                    AddAppToBackGroud(appVm);
                }
                else if (appVm.AppInfoModel.AppPath != config.Value.AppPath)
                {
                    appVm.AppInfoModel.Update(config.Value);
                }
            }
        }

        private void RemoveAppsNotExists(Dictionary<string, ProgramPathModel> newConfigs)
        {
            var toRemove = _appBackgrounds.Where(a => !newConfigs.ContainsKey(a.Key)).ToList();
            foreach (var app in toRemove)
            {
                app.Value.StopUpdate();
                _appBackgrounds.Remove(app.Key);
            }
        }
    }
}
