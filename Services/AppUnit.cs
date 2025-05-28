using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Configs;
using UiStore.Model;
using UiStore.Models;
using UiStore.View;

namespace UiStore.Services
{

    internal class AppUnit
    {
        private readonly AppUpdater _updater;
        private AppModel appModel;

        private string _programFolderPath;
        public string ProgramFolderPath { get => _programFolderPath; set { _updater.ProgramFolderPath = value; _programFolderPath = value; } }
        public string CommonFolderPath { get => _updater.CommonFolderPath; set { _updater.CommonFolderPath = value; } }

        private bool _isRunning;
        public bool IsRunning { get => _isRunning; private set { _isRunning = value; OnStatusChanged?.Invoke(); } }

        private int _status;
        public int Status { get => _status; set { _status = value; OnStatusChanged?.Invoke(); } }

        private string _name;
        public string Name { get => _name; set { _name = value; _updater.Name = value; } }
        public string AppModelPath { get; set; }
        public string Version { get; internal set; }
        public string LocalPath { get; internal set; }

        public Action RemoveAppAction { get; internal set; }
        public Action AddAppAction { get; internal set; }
        public Action OnStatusChanged { get; internal set; }
        public Predicate<Dictionary<string, string>> IsCanOpen { get; set; }

        public event Action<string> OnLog;
        public event Action<int> OnProgress;
        public event Action<string> OnIconFileChanged;
        private CancellationTokenSource _cts;

        internal AppUnit(CacheManager cache)
        {
            _updater = new AppUpdater(cache);
            IsRunning = false;
        }

        internal void Init()
        {
            _updater.OnLog += msg => OnLog?.Invoke(msg);
            _updater.OnProgress += p => OnProgress?.Invoke(p);
            _updater.OnStatus += st => Status = st;
        }

        public void LaunchApp()
        {
            try
            {
                if (appModel == null || IsRunning || Status != ConstKey.AppState.STANDBY_STATE) return;
                Dictionary<string, string> accounts = AutoDLConfig.ConfigModel.Accounts;
                if (IsCanOpen?.Invoke(accounts) != true)
                {
                    return;
                }
                Task.Run(async () =>
                {
                    try
                    {
                        IsRunning = true;
                        if (await _updater.CreateProgram(appModel))
                        {
                            string cmd = $"cd \"{ProgramFolderPath}\" && {appModel.OpenCmd}";
                            Util.RunCmd(cmd);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnLog?.Invoke($"[{Name}] Lỗi mở app: {ex.Message}");
                    }
                    finally
                    {
                        Directory.Delete(ProgramFolderPath, true);
                        IsRunning = false;
                    }
                });
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"[{Name}] Lỗi mở app: {ex.Message}");
            }
        }

        internal void StartUpdate()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (IsRunning)
                        {
                            continue;
                        }
                        if (AppModelPath == null)
                        {
                            StopUpdate();
                            RemoveAppAction?.Invoke();
                        }
                        appModel = await TranforUtil.GetModelConfig<AppModel>(AppModelPath);
                        if (appModel == null)
                        {
                            StopUpdate();
                            RemoveAppAction?.Invoke();
                            OnLog?.Invoke($"Lỗi[{Name}]: Không lấy được thông tin từ server!!");
                        }
                        else if (appModel.Enable && appModel.FileModels?.Count > 0 && Status == ConstKey.AppState.STANDBY_STATE)
                        {
                            AddAppAction?.Invoke();
                            if (!string.IsNullOrEmpty(appModel.MainPath))
                            {
                                var iconFile = appModel.FileModels.FirstOrDefault(f => Util.ArePathsEqual(f.ProgramPath, appModel.MainPath));
                                if (iconFile != null)
                                {
                                    var rs = await _updater.CheckSumAppFiles(iconFile);
                                    OnIconFileChanged?.Invoke(rs.Item2);
                                }
                            }
                            await _updater.UpdateAsync(appModel);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnLog?.Invoke($"Lỗi[{Name}]:{ex.Message}");
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(AutoDLConfig.ConfigModel.UpdateTime), _cts.Token);
                    }
                }
            }, _cts.Token);
        }

        internal void StopUpdate()
        {
            _cts?.Cancel();
        }
    }
}
