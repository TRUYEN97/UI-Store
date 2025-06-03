using System;
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
        public ObservableCollection<string> Ips { get; } = new ObservableCollection<string>();

        private readonly Location _location;
        private readonly Logger _logger;
        private readonly Authentication _authentication;
        private readonly ProgramManagement _programManagement;
        private readonly MyTimer _timer;
        private CancellationTokenSource _cts;
        private readonly object _lock = new object();

        public MainViewModel(CacheManager cache)
        {
            _location = AutoDLConfig.ConfigModel.Location;
            PcName = PcInfo.PcName;
            Product = _location.Product;
            Station = _location.Station;
            _logger = new Logger();
            _programManagement = new ProgramManagement(cache, this._logger);
            _authentication = new Authentication(this._logger, PathUtil.GetStationAccessUserPath(_location));
            Title = $"{ProgramInfo.ProductName} - V{ProgramInfo.ProductVersion}";
            _timer = new MyTimer((_) =>
            {
                UpdateIpsSafe();
            });
        }
        public ObservableCollection<AppViewModel> Applications => _programManagement.Applications;
        public ObservableCollection<string> LogLines => _logger.LogLines;
        public string PcName { get; private set; }
        public string Title { get; private set; }
        public string Product { get; private set; }
        public string Station { get; private set; }

        public void Start()
        {
            _timer?.Start(0, 3000);
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => LoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel(); 
            _timer?.Stop();
        }

        private async Task LoopAsync(CancellationToken token)
        {
            int updateTime = AutoDLConfig.ConfigModel.UpdateTime;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_authentication.IsAuthenticated || await _authentication.Login(true))
                    {
                        await SyncConfigAsync();
                        await Task.Delay(TimeSpan.FromSeconds(updateTime), token);
                    }
                    else { Application.Current.Shutdown(); }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    _logger.AddLogLine(ex.Message);
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
                    _programManagement.UpdateApps(newConfigs);
                }
            }
        }
    }


}
