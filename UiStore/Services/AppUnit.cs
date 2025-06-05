using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Models;
using UiStore.ViewModel;

namespace UiStore.Services
{

    internal class AppUnit : IDisposable
    {
        private readonly AppUpdater _updater;
        private readonly AppViewModel _appView;
        private readonly Logger _logger;
        private readonly ProgramManagement _programManagement;
        private readonly Authentication authentication;
        private readonly MyTimer _AutoOpentimer;
        private readonly MyTimer _AutoDisabletimer;
        private CancellationTokenSource _cts;
        private int _doStatus;
        private int _appStatus;
        private bool _isRunning;
        private readonly object _lock;

        internal AppUnit(CacheManager cache, ProgramManagement programManagement, ProgramPathModel programPathModel, AppViewModel app, Logger logger)
        {
            _updater = new AppUpdater(cache, this, logger);
            _programManagement = programManagement;
            authentication = new Authentication(logger);
            AppInfoModel = new AppInfoModel(programPathModel);
            _appView = app;
            _logger = logger;
            _lock = new object();
            _AutoOpentimer = new MyTimer((a) =>
            {
                if (IsRunanble())
                {
                    LaunchApp();
                }
            });
            _AutoDisabletimer = new MyTimer((a) =>
            {
                if (!IsRunning)
                {
                    _programManagement.DisableApp(app);
                }
            });
        }

        public AppInfoModel AppInfoModel { get; private set; }
        public AppModel AppModel { get; private set; }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                _appView.RunningStatus(value);
            }
        }
        public int DoStatus
        {
            get => _doStatus;
            set
            {
                _doStatus = value;
                _appView.DoStatus(DoStatus);
            }
        }

        public int AppStatus { get => _appStatus; internal set { _appStatus = value; this._appView.AppStatus(value); } }

        public void SetProgress(int progress)
        {
            this._appView.Progress = progress;
        }

        internal void CloseApp()
        {
            if (IsRunning)
            {
                //string cmd = $"cd \"{AppInfoModel.ProgramFolderPath}\" && {AppModel.CloseCmd}";
                //Util.RunCmd(cmd);
            }
        }

        public void LaunchApp()
        {
            try
            {
                lock (_lock)
                {
                    if (string.IsNullOrWhiteSpace(AppInfoModel?.AppPath) || AppModel == null || !IsRunanble()) return;
                    Task.Run(async () =>
                    {
                        authentication.AccessUserListModelPath = AppInfoModel?.AccectUserPath;
                        if (await authentication.Login() && IsRunanble())
                        {
                            try
                            {

                                IsRunning = true;
                                if (await _updater.CreateProgram(AppModel))
                                {
                                    string cmd = $"cd \"{AppInfoModel.ProgramFolderPath}\" && {AppModel.OpenCmd}";
                                    Util.RunCmd(cmd);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.AddLogLine(($"[{AppInfoModel.Name}]: {ex.Message}"));
                            }
                            finally
                            {
                                IsRunning = false;
                                if (AppModel?.CloseAndClear == true)
                                {
                                    _programManagement.RemoveProgramFolder(AppInfoModel);
                                }
                                else
                                {
                                    _programManagement.RemoveProgram(_updater.FilesToRemove);
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.AddLogLine($"[{AppInfoModel.Name}]: {ex.Message}");
            }
        }

        internal void StartUpdate()
        {
            if (_cts != null && _cts.Token.IsCancellationRequested) return;
            _cts = new CancellationTokenSource();
            _programManagement.RemoveAppIcon(AppInfoModel);
            Task.Run(async () =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            if (AppInfoModel?.AppPath == null)
                            {
                                StopUpdate();
                            }
                            AppModel = await TranforUtil.GetModelConfig<AppModel>(AppInfoModel?.AppPath, ConstKey.ZIP_PASSWORD);
                            if (!AppModel.Enable || AppModel.FileModels == null || AppModel.FileModels.Count == 0)
                            {
                                _AutoDisabletimer.Start(0, 2000);
                                AppStatus = ConstKey.AppStatus.DELETED;
                            }
                            else
                            {
                                AppStatus = ConstKey.AppStatus.STANDBY;
                                _AutoDisabletimer.Stop();
                                _appView.UpdateInfoForm();
                                _programManagement.AddApp(_appView);
                                InitStorePathFor(AppModel);
                                await _updater.CheckUpdate(AppModel);
                                if (AppModel.AutoOpen)
                                {
                                    _AutoOpentimer.Start(0, 1000);
                                }
                                else
                                {
                                    _AutoOpentimer.Stop();
                                }
                            }
                            await Task.Delay(TimeSpan.FromSeconds(AutoDLConfig.ConfigModel.UpdateTime), _cts.Token);
                        }
                        catch (Exception ex)
                        {
                            _logger.AddLogLine($"[{AppInfoModel.Name}]: {ex.Message}");
                            StopUpdate();
                        }
                    }
                }
                finally
                {
                    _programManagement.RemoveApp(_appView, AppModel);
                }

            }, _cts.Token);
        }

        private void InitStorePathFor(AppModel appModel)
        {
            string rootFolderPath = AppInfoModel.ProgramFolderPath;
            foreach (var file in appModel.FileModels)
            {
                string storeFile = file.ProgramPath;
                if (!string.IsNullOrWhiteSpace(rootFolderPath))
                {
                    storeFile = Path.Combine(rootFolderPath, file.ProgramPath);
                }
                file.StorePath = storeFile;
            }
        }

        private bool IsRunanble()
        {
            return !IsRunning && DoStatus == ConstKey.DoStatus.DO_NOTHING && AppStatus == ConstKey.AppStatus.STANDBY;
        }

        internal void StopUpdate()
        {
            _cts?.Cancel();
        }

        public void Dispose()
        {
            _AutoOpentimer?.Dispose();
            _AutoDisabletimer?.Dispose();
        }

        internal void ExtractIconFromApp(string iconPath)
        {
            _appView.ExtractIconFromApp(iconPath);
        }
    }
}
