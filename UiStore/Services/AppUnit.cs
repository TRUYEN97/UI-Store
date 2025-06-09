using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UiStore.Configs;
using UiStore.Models;
using UiStore.ViewModel;

namespace UiStore.Services
{

    internal class AppUnit
    {
        private readonly InstanceWarehouse _instanceWarehouse;
        private readonly AppModelManagement _appModelManagement;
        private readonly AppStoreFileManagement _appStoreFileManagement;
        private readonly AppViewModel _appView;
        private readonly ProgramManagement _programManagement;
        private readonly AppEvent _appEvent;
        private readonly AppStatusInfo _appStatusInfo;
        private readonly AppInfoModel _appInfoModel;
        private readonly AppAttack _updater;
        private readonly Authentication _authentication;
        private readonly Logger _logger;
        private CancellationTokenSource _cts;
        private readonly object _lock;

        internal AppUnit(CacheManager cache, ProgramManagement programManagement, AppInfoModel appInfoModel, AppViewModel appView, Logger logger)
        {
            _programManagement = programManagement;
            _appInfoModel = appInfoModel;
            _appView = appView;
            _logger = logger;
            _instanceWarehouse = new InstanceWarehouse();
            _appEvent = new AppEvent(logger, _instanceWarehouse);
            _appStatusInfo = new AppStatusInfo(_appEvent);
            _appModelManagement = new AppModelManagement(appInfoModel, _appStatusInfo);
            _appStoreFileManagement = new AppStoreFileManagement(_appInfoModel, _appModelManagement);
            _updater = new AppAttack(cache, _instanceWarehouse, logger);
            _authentication = new Authentication(logger)
            {
                AccessUserListModelPath = _appInfoModel?.AccectUserPath
            };
            _lock = new object();
            _instanceWarehouse.AppStatusInfo = _appStatusInfo;
            _instanceWarehouse.AppInfoModel = _appInfoModel;
            _instanceWarehouse.AppUnit = this;
            _instanceWarehouse.AppViewModel = appView;
            _instanceWarehouse.AppModelManagement = _appModelManagement;
            _instanceWarehouse.AppStoreFileManagement = _appStoreFileManagement;
            _instanceWarehouse.ProgramManagement = _programManagement;
        }

        public InstanceWarehouse InstanceWarehouse => _instanceWarehouse;

        internal void CloseApp()
        {
            if (_appStatusInfo.IsRunning)
            {
                //string cmd = $"cd \"{AppInfoModel.ProgramFolderPath}\" && {AppModel.CloseCmd}";
                //Util.RunCmd(cmd);
            }
        }

        public void LaunchApp()
        {
            try
            {
                if (!_appStatusInfo.IsRunnable) return;
                lock (_lock)
                {
                    if (!_appStatusInfo.IsRunnable) return;
                    Task.Run(async () =>
                    {
                        if (await _authentication.Login() && _appStatusInfo.IsRunnable)
                        {
                            await _updater.Open();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine(ex.Message);
            }
        }

        internal void StartUpdate()
        {
            if (_cts != null && _cts.Token.IsCancellationRequested) return;
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await _updater.CheckUpdate();
                            await Task.Delay(TimeSpan.FromSeconds(AutoDLConfig.ConfigModel.UpdateTime), _cts.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.AddLogLine(ex.Message);
                            StopUpdate();
                        }
                    }
                }
                finally
                {
                    _appStatusInfo.IsAppAvailable = false;
                }

            }, _cts.Token);
        }

        internal void StopUpdate()
        {
            _cts?.Cancel();
        }

        internal void ExtractIconFromApp(string iconPath)
        {
            _appView.ExtractIconFromApp(iconPath);
        }
    }
}
