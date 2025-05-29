using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Services;
using UiStore.ViewModels;

namespace UiStore.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public ObservableCollection<AppViewModel> Applications { get; } = new ObservableCollection<AppViewModel>();
        public ObservableCollection<string> LogLines { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Ips { get; } = new ObservableCollection<string>();

        private readonly CacheManager _cache;
        private readonly Location _location;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();

        public MainViewModel(CacheManager cache)
        {
            _cache = cache;
            _location = AutoDLConfig.ConfigModel.Location;
            PcName = PcInfo.PcName;
            Product = _location.Product;
            Station = _location.Station;
            Title = $"{ProgramInfo.ProductName} - V{ProgramInfo.ProductVersion}";
        }

        public string PcName { get; private set; }
        public string Title { get; private set; }
        public string Product { get; private set; }
        public string Station { get; private set; }

        public void AddLogLine(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogLines.Add($"{DateTime.Now:HH:mm:ss} -> {message}");
                while (LogLines.Count > 15)
                {
                    LogLines.RemoveAt(0);
                }
            });
        }

        public void RemoveApp(AppViewModel appViewModel)
        {
            DispatcherHelper.RunOnUI(() => Applications.Remove(appViewModel));
        }

        private bool IsContainApp(AppViewModel appViewModel)
        {
            foreach (var app in Applications)
            {
                if (app == appViewModel || app.AppModelPath == appViewModel?.AppModelPath)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddApp(AppViewModel appViewModel)
        {
            DispatcherHelper.RunOnUI(() =>
            {
                if (!IsContainApp(appViewModel))
                {
                    Applications.Add(appViewModel);
                }
            });
        }

        public void Start()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private async Task LoopAsync(CancellationToken token)
        {
            int updateTime = AutoDLConfig.ConfigModel.UpdateTime;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    UpdateIpsSafe();
                    await SyncConfigAsync();
                    await Task.Delay(TimeSpan.FromSeconds(updateTime), token);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    AddLogLine($"Lỗi: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
        }

        private void UpdateIpsSafe()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Ips.Clear();
                foreach (var ip in PcInfo.GetIps)
                {
                    Ips.Add(ip);
                }
            });
        }

        private async Task SyncConfigAsync()
        {
            var appList = (await TranforUtil.GetAppListModel(_location, ConstKey.ZIP_PASSWORD)).Item1;
            if (appList?.ProgramPaths?.Count > 0)
            {
                var newConfigs = appList.ProgramPaths.ToDictionary(elem => Util.GetAppName(_location, elem.Key), elm => elm.Value);
                lock (_lock)
                {
                    RemoveAppsNotExists(newConfigs);
                    UpdateApps(newConfigs);
                }
            }
        }

        private void UpdateApps(Dictionary<string, string> newConfigs)
        {
            var existingApps = Applications.ToDictionary(a => a.Name, a => a);
            foreach (var config in newConfigs)
            {
                if (!existingApps.TryGetValue(config.Key, out var appVm))
                {
                    string name = config.Key;
                    appVm = new AppViewModel(_cache)
                    {
                        Name = name,
                        ProgramFolderPath = $"{AutoDLConfig.ConfigModel.AppLocalPath}/{name}",
                        CommonFolderPath = $"{AutoDLConfig.ConfigModel.CommonLocalPath}",
                        AppModelPath = config.Value
                    };
                    appVm.Init(AddLogLine, AddApp, RemoveApp);
                    appVm.StartUpdate();
                }
                else if (appVm.AppModelPath != config.Value)
                {
                    appVm.StopUpdate();
                    appVm.AppModelPath = config.Value;
                    appVm.StartUpdate();
                }
            }
        }

        private void RemoveAppsNotExists(Dictionary<string, string> newConfigs)
        {
            var toRemove = Applications.Where(a => !newConfigs.ContainsKey(a.Name)).ToList();
            foreach (var app in toRemove)
            {
                app.StopUpdate();
                RemoveApp(app);
            }
        }
    }


}
