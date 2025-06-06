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
        private readonly Authentication _authentication;
        private CancellationTokenSource _cts;
        private readonly object _lock;

        internal AppUnit(CacheManager cache, ProgramManagement programManagement, ProgramPathModel programPathModel, AppViewModel app, Logger logger)
        {
            AppEvent = new AppEvent();
            AppStatusInfo = new AppStatusInfo(AppEvent);
            _updater = new AppUpdater(cache, this);
            ProgramManagement = programManagement;
            _authentication = new Authentication(logger);
            AppInfoModel = new AppInfoModel(programPathModel);
            AppView = app;
            Logger = logger;
            _lock = new object();
            AutoOpentimer = new MyTimer((_) =>
            {
                if (AppStatusInfo.IsRunnable)
                {
                    LaunchApp();
                }
            });
            AutoDisabletimer = new MyTimer((_) =>
            {
                if (!AppStatusInfo.IsRunning)
                {
                    ProgramManagement.DisableApp(AppView);
                }
            });
            InitEventAction();
        }

        public AppViewModel AppView;

        public Logger Logger {  get; private set; }

        public ProgramManagement ProgramManagement {  get; private set; }
        public AppEvent AppEvent { get; private set; }
        public AppStatusInfo AppStatusInfo { get; private set; }
        public AppInfoModel AppInfoModel { get; private set; }
        public AppModel AppModel => _updater.CurrentAppModel;
        public MyTimer AutoOpentimer { get; private set; }
        public MyTimer AutoDisabletimer { get; private set; }


        public void SetProgress(int progress)
        {
            this.AppView.Progress = progress;
        }

        internal void CloseApp()
        {
            if (AppStatusInfo.IsRunning)
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
                    if (!AppStatusInfo.IsRunnable) return;
                    Task.Run(async () =>
                    {
                        _authentication.AccessUserListModelPath = AppInfoModel?.AccectUserPath;
                        if (await _authentication.Login() && AppStatusInfo.IsRunnable)
                        {
                            try
                            {

                                AppStatusInfo.IsRunning = true;
                                if (await _updater.CreateProgram(AppModel))
                                {
                                    string cmd = $"cd \"{AppInfoModel.ProgramFolderPath}\" && {AppModel.OpenCmd}";
                                    Util.RunCmd(cmd);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.AddLogLine(($"[{AppInfoModel.Name}]: {ex.Message}"));
                            }
                            finally
                            {
                                AppStatusInfo.IsRunning = false;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.AddLogLine($"[{AppInfoModel.Name}]: {ex.Message}");
            }
        }

        internal void StartUpdate()
        {
            if (_cts != null && _cts.Token.IsCancellationRequested) return;
            _cts = new CancellationTokenSource();
            ProgramManagement.RemoveAppIcon(AppInfoModel);
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
                            Logger.AddLogLine($"[{AppInfoModel.Name}]: {ex.Message}");
                            StopUpdate();
                        }
                    }
                }
                finally
                {
                    ProgramManagement.RemoveApp(AppView, AppModel);
                }

            }, _cts.Token);
        }

        internal void StopUpdate()
        {
            _cts?.Cancel();
        }

        public void Dispose()
        {
            AutoOpentimer?.Dispose();
            AutoDisabletimer?.Dispose();
        }

        private void InitEventAction()
        {
            AppEvent.EnableStatus.AddAction("AppEnableName", (b) =>
            {
                if (b)
                {
                    ProgramManagement.AddApp(AppView, AppModel);
                    AutoDisabletimer.Stop();
                }
                else
                {
                    AutoDisabletimer.Start(0, 2000);
                }
            });
            AppEvent.ActiveStatus.AddAction("AppDisableActionName", (b) =>
            {
                if (!b)
                {
                    StopUpdate();
                }
            });
            AppEvent.AutoRunAction.AddAction("AutoRunActionName", (b) =>
            {
                if (!b)
                    AutoOpentimer.Stop();
                else
                    AutoOpentimer.Start(0, 2000);
            });
            AppEvent.RunningStatus.AddAction("CloseAndClear", (b) =>
            {
                if (b.Item1) return;
                if (b.Item2)
                    ProgramManagement.RemoveProgramFolder(AppInfoModel);
                else
                    ProgramManagement.RemoveProgram(_updater.FilesToRemove);
            });
        }

        internal void ExtractIconFromApp(string iconPath)
        {
            AppView.ExtractIconFromApp(iconPath);
        }
    }
}
