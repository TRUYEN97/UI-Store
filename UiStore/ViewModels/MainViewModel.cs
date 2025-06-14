using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Models;
using UiStore.Services;
using UiStore.ViewModels;

namespace UiStore.ViewModel
{
    internal class MainViewModel : BaseViewModel
    {
        public ObservableCollection<string> Ips { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> LogLines { get; } = new ObservableCollection<string>();
        private readonly Location _location;
        private readonly Logger _mainLogger;
        private readonly Authorization _authorization;
        private readonly ProgramManagement _programManagement;
        private readonly MyTimer _timer;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();

        public MainViewModel()
        {
            _location = AutoDLConfig.ConfigModel.Location;
            PcName = PcInfo.PcName;
            Product = _location.Product;
            Station = _location.Station;
            _mainLogger = new Logger(LogLines, "Ui Store");
            var cache = new CacheManager(_mainLogger);
            cache.LoadFromFolder(AutoDLConfig.ConfigModel.CommonLocalPath);
            _programManagement = new ProgramManagement(cache, _mainLogger);
            _authorization = new Authorization(_mainLogger, PathUtil.GetStationAccessUserPath(_location));
            Title = $"{ProgramInfo.ProductName} - V{ProgramInfo.ProductVersion}";
            _timer = new MyTimer((_) =>
            {
                UpdateIpsSafe();
            });
        }
        public ObservableCollection<AppViewModel> Applications => _programManagement.Applications;
        public string PcName { get; private set; }
        public string Title { get; private set; }
        public string Product { get; private set; }
        public string Station { get; private set; }

        public void Start()
        {
            _timer?.Start(0, 5000);
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                await CheckConnectServer(_cts.Token);
                await CheckLogin(_cts.Token);
                await LoopAsync(_cts.Token);
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _timer?.Stop();
        }

        private async Task CheckConnectServer(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_authorization.IsConnected)
            {
                try
                {
                    _mainLogger.AddLogLine("Connect to server failded!");
                }
                catch (Exception ex)
                {
                    _mainLogger.AddLogLine(ex.Message);
                }
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }

        private async Task CheckLogin(CancellationToken token)
        {
            if (!token.IsCancellationRequested && !await _authorization.Login())
            {
                DispatcherHelper.RunOnUI(() =>
                {
                    Application.Current.Shutdown();
                });
            }
        }

        private async Task LoopAsync(CancellationToken token)
        {
            int updateTime = AutoDLConfig.ConfigModel.UpdateTime;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await SyncConfigAsync();
                    await Task.Delay(TimeSpan.FromSeconds(updateTime), token);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    _mainLogger.AddLogLine(ex.Message);
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
            try
            {
                string appConfigRemotePath = PathUtil.GetAppConfigRemotePath(_location);
                var appList = await TranforUtil.GetModelConfig<AppList>(appConfigRemotePath, ConstKey.ZIP_PASSWORD);
                if (appList?.ProgramPaths?.Count > 0)
                {
                    lock (_lock)
                    {
                        _programManagement.UpdateApps(appList.ProgramPaths);
                    }
                }
            }
            catch (ConnectFaildedException ex)
            {
                _mainLogger.AddLogLine(ex.Message);
            }
            catch (SftpFileNotFoundException ex)
            {
                _mainLogger.AddLogLine(ex.Message);
            }
        }
    }
}
