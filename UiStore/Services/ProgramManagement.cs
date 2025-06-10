using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Models;
using UiStore.ViewModel;

namespace UiStore.Services
{
    internal class ProgramManagement
    {
        public ObservableCollection<AppViewModel> Applications { get; } = new ObservableCollection<AppViewModel>();
        private readonly ConcurrentDictionary<string, AppViewModel> _appBackgrounds = new ConcurrentDictionary<string, AppViewModel>();
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
                });
                _logger.AddLogLine($"Add [{appViewModel.Name}]");
            }
        }

        public void RemoveApp(AppViewModel appViewModel)
        {
            DisableApp(appViewModel);
            if (_appBackgrounds.ContainsKey(appViewModel.Name))
            {
                _appBackgrounds.TryRemove(appViewModel.Name, out _);
                _logger.AddLogLine($"Remove [{appViewModel.Name}]");
                _cache.TryRemoveWith(appViewModel.Name);
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
                _appBackgrounds.TryAdd(name, app);
                app.StartUpdate();
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
                string name = config.Key;
                ProgramPathModel pathModel = config.Value;
                if (!TryGetBackGroupApp(name, out var appVm))
                {
                    var AppInfoModel = new AppInfoModel(pathModel)
                    {
                        Name = name,
                        RootDir = AutoDLConfig.ConfigModel.AppLocalPath,
                        CommonFolderPath = AutoDLConfig.ConfigModel.CommonLocalPath
                    };
                    appVm = new AppViewModel(_cache, this, AppInfoModel, _logger.CreateNew(name));
                    AddAppToBackGroud(appVm);
                }
                else if (appVm.AppInfoModel.AppPath != pathModel.AppPath)
                {
                    appVm.AppInfoModel.Update(pathModel);
                }
            }
        }

        private void RemoveAppsNotExists(Dictionary<string, ProgramPathModel> newConfigs)
        {
            var toRemove = _appBackgrounds.Where(a => !newConfigs.ContainsKey(a.Key)).ToList();
            foreach (var app in toRemove)
            {
                app.Value.StopUpdate();
                _appBackgrounds.TryRemove(app.Key, out _);
            }
        }
    }
}
