using System;
using UiStore.Models;
using UiStore.ViewModel;
using static UiStore.Services.AppStatusInfo;

namespace UiStore.Services
{
    internal class AppEvent
    {
        private readonly InstanceWarehouse _instanceWarehouse;
        private const string SHOW_RUNNING_STATUS = "Show Running Status";
        private const string CLEAR_ICON_PATH = "Clear Icon Path for new";
        private const string SHOW_UPDDATE_STATUS = "Show Update status";
        private const string SHOW_DELETE_SATAUS = "Show delete Status";
        private const string CLEAR_STORE = "Clear Store Useless Files";
        private const string SHOW_NEW_VERSION_STATUS = "New Version";
        private const string SHOW_MESSAGE_LOG = "Show Message";
        private const string ENABLE = "Enable";
        private const string UPDATE_INFO = "Update app info";
        private const string PROGESS_CHANGED = "Progress Changed";
        private const string AUTO_RUN_MODE = "Auto Run";
        private const string UPDATE_CURRENT_MODEL = "Update current model";
        private const string UPDATE_USE_MODEL = "Update use model";
        private const string REMOVE_NEW_VERSION_STATUS = "remove new version status";
        private const string REFRESH_APP_INFO = "Refresh App Info";
        private const string SET_NEED_UPDATE = "Set Need Update status";
        private const string RESET_NEED_UPDATE = "Reset Need Update status";
        private const string CLOSE_AND_CLEAR = "Close And Clear";
        private const string CLEAR_REMOVER_APP_FILE_ACTION = "clear_remover_app_file_action";
        private readonly Logger _logger;

        public AppEvent(Logger logger, InstanceWarehouse instance)
        {
            _logger = logger;
            _instanceWarehouse = instance;
            ActiveStatus = new OnAppAction<bool, AppEvent>(this);
            HasUpdate = new OnAppAction<bool, AppEvent>(this);
            EnableStatus = new OnAppAction<bool, AppEvent>(this);
            AutoRunAction = new OnAppAction<bool, AppEvent>(this);
            ExtractAction = new OnAppAction<ExtractState, AppEvent>(this);
            UpdateAction = new OnAppAction<UpdateState, AppEvent>(this);
            HasNewVersion = new OnAppAction<bool, AppEvent>(this);
            RunningStatus = new OnAppAction<bool, AppEvent>(this);
            Progress = new OnAppAction<int, AppEvent>(this);
            InitHasUpdateAction();
            InitProgressAction();
            InitNewVersionAction();
            InitExtractAction();
            InitUpdateAction();
            InitRunningAction();
            InitAutoRunAction();
            InitEnableStatusAction();
            InitActiveStatusAction();
        }
        public InstanceWarehouse InstanceWarehouse { get { return _instanceWarehouse; } }
        public OnAppAction<bool, AppEvent> ActiveStatus { get; private set; }
        public OnAppAction<bool, AppEvent> HasUpdate { get; private set; }
        public OnAppAction<bool, AppEvent> EnableStatus { get; private set; }
        public OnAppAction<bool, AppEvent> AutoRunAction { get; private set; }
        public OnAppAction<UpdateState, AppEvent> UpdateAction { get; private set; }
        public OnAppAction<ExtractState, AppEvent> ExtractAction { get; private set; }
        public OnAppAction<bool, AppEvent> HasNewVersion { get; private set; }
        public OnAppAction<bool, AppEvent> RunningStatus { get; private set; }
        public OnAppAction<int, AppEvent> Progress { get; private set; }

        public AppViewModel AppView => InstanceWarehouse.AppViewModel;
        public AppModelManagement AppModelManage => InstanceWarehouse.AppModelManagement;
        public AppInfoModel AppInfoModel => InstanceWarehouse.AppInfoModel;
        public AppUnit AppUnit => InstanceWarehouse.AppUnit;
        public AppStoreFileManagement AppStoreFile => InstanceWarehouse.AppStoreFileManagement;
        public AppStatusInfo AppStatusInfo => InstanceWarehouse.AppStatusInfo;
        public ProgramManagement ProgramManagement => InstanceWarehouse.ProgramManagement;

        private void InitHasUpdateAction()
        {
            HasUpdate.Add(new RelayAction<bool, AppEvent>(
                REMOVE_NEW_VERSION_STATUS,
                (hasUpdate, _) => hasUpdate && !RunningStatus.Value,
                (hasUpdate, _) => HasNewVersion.SetValue(false))
                );
        }

        private void InitProgressAction()
        {
            Progress.Add(new RelayAction<int, AppEvent>(
                PROGESS_CHANGED,
                (value, _) => AppView.Progress = value));
        }
        private void InitNewVersionAction()
        {
            HasNewVersion.Add(new RelayAction<bool, AppEvent>(
                SHOW_NEW_VERSION_STATUS,
                (value, _) => AppView.SetNewVersionStatus(value)));

            HasNewVersion.Add(new RelayAction<bool, AppEvent>(
                SET_NEED_UPDATE,
                (value, _) => value,
                (value, _) => AppView.SetNewVersionStatus(value)));

            HasNewVersion.Add(
                new RelayAction<bool, AppEvent>(
                SHOW_MESSAGE_LOG,
                (value, _) => value,
                (value, _) => _logger.AddLogLine("The program has a new version!")));
        }
        private void InitExtractAction()
        {
            ExtractAction.Add(new RelayAction<ExtractState, AppEvent>(
                UPDATE_USE_MODEL,
                (value, _) => value == ExtractState.SUCCESS,
                (value, _) => AppModelManage.UpdateUseModel()));

            ExtractAction.Add(new RelayAction<ExtractState, AppEvent>(
               REFRESH_APP_INFO,
               (value, ins) => AppView.RefreshAppInfo()));

            ExtractAction.Add(new RelayAction<ExtractState, AppEvent>(
                SHOW_MESSAGE_LOG,
                (status, _) =>
                {
                    switch (status)
                    {
                        case ExtractState.SUCCESS:
                            _logger.AddLogLine("Extract success!");
                            break;
                        case ExtractState.FAILED:
                            _logger.AddLogLine("Extract failure!");
                            break;
                        case ExtractState.EXTRACTING:
                            _logger.AddLogLine("Extracting...");
                            break;
                    }
                }));
        }
        private void InitUpdateAction()
        {
            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               UPDATE_CURRENT_MODEL,
               (value, _) => value == UpdateState.SUCCESS,
               (value, ins) => ins.AppModelManage.UpdateCurrentModel()));

            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               RESET_NEED_UPDATE,
               (value, _) => value == UpdateState.SUCCESS,
               (value, ins) => HasUpdate.SetValue(true)));

            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               AUTO_RUN_MODE,
               (value, _) => value == UpdateState.SUCCESS,
               (value, ins) => AutoRunAction.RunActions()));

            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               SHOW_UPDDATE_STATUS, (value, _) => AppView.SetUpdateStatus(value)));

            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               UPDATE_INFO,
               (value, _) => value == UpdateState.SUCCESS && !HasNewVersion.Value && AppModelManage?.CurrentAppModel != null,
               (value, ins) =>
               {
                   AppView.FWVersion = AppModelManage.CurrentAppModel?.FTUVersion;
                   AppView.FCDVersion = AppModelManage.CurrentAppModel?.FCDVersion;
                   AppView.BOMVersion = AppModelManage.CurrentAppModel?.BOMVersion;
                   AppView.FTUVersion = AppModelManage.CurrentAppModel?.FTUVersion;
                   AppView.Version = AppModelManage.CurrentAppModel?.Version;
               }));

            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               REFRESH_APP_INFO,
               (value, ins) => AppView.RefreshAppInfo()));

            UpdateAction.Add(new RelayAction<UpdateState, AppEvent>(
               SHOW_MESSAGE_LOG,
               (status, _) => EnableStatus.Value,
               (status, ins) =>
               {
                   switch (status)
                   {
                       case UpdateState.SUCCESS:
                           //_logger.AddLogLine("Update success!");
                           break;
                       case UpdateState.FAILED:
                           _logger.AddLogLine("Update failure!");
                           break;
                       case UpdateState.UPDATING:
                           //_logger.AddLogLine("Updating..!");
                           break;
                   }
               }));

        }

        private void InitRunningAction()
        {
            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               REMOVE_NEW_VERSION_STATUS,
               (value, ins) => HasUpdate.RunActions()));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               CLEAR_REMOVER_APP_FILE_ACTION,
               (value, ins) => UpdateAction.ClearQueue()));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               REFRESH_APP_INFO,
               (value, ins) => AppView.RefreshAppInfo()));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               SHOW_MESSAGE_LOG,
               (value, ins) => { if (value) _logger.AddLogLine("Running..."); else _logger.AddLogLine("Stop run"); }));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
                CLEAR_STORE,
                (value, ins) => !value,
                (value, ins) =>
                {
                    if (AppStatusInfo.IsStandby)
                    {
                        AppStoreFile.CleanStoreFileModel();
                    }
                    else
                    {
                        UpdateAction.AddOneTimeAction(new RelayAction<UpdateState, AppEvent>(
                            CLEAR_STORE,
                            (_, o) => AppStatusInfo.IsStandby,
                            (_, o) => AppStoreFile.CleanStoreFileModel()));
                    }
                }));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               SHOW_RUNNING_STATUS,
               (value, ins) => AppView.SetRunning(value)));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               CLOSE_AND_CLEAR,
               (value, ins) => !value && AppStatusInfo.IsCloseAndClear,
               (value, ins) =>
               {
                   if (AppStatusInfo.IsStandby)
                   {
                       AppStoreFile.RemoveProgramFolder();
                   }
                   else
                   {
                       UpdateAction.AddOneTimeAction(new RelayAction<UpdateState, AppEvent>(
                           CLOSE_AND_CLEAR,
                           (_, o) => AppStatusInfo.IsStandby,
                           (_, o) => AppStoreFile.RemoveProgramFolder()));
                   }
               }));
            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               AUTO_RUN_MODE,
               (value, ins) => AutoRunAction.RunActions()));

            RunningStatus.Add(new RelayAction<bool, AppEvent>(
               ENABLE,
               (value, ins) => !value,
               (value, ins) =>
               {
                   if (!AppStatusInfo.IsAppAvailable)
                   {
                       ProgramManagement.RemoveApp(AppView);
                   }
                   else if (!AppStatusInfo.IsEnable)
                   {
                       ProgramManagement.DisableApp(AppView);
                   }
               }));
        }

        private void InitAutoRunAction()
        {
            AutoRunAction.Add(new RelayAction<bool, AppEvent>(
               AUTO_RUN_MODE,
               (value, ins) => value && !RunningStatus.Value && AppStatusInfo.IsRunnable,
               (value, ins) => AppUnit.LaunchApp()));
        }
        private void InitEnableStatusAction()
        {
            EnableStatus.Add(new RelayAction<bool, AppEvent>(
               ENABLE,
               (value, ins) =>
               {
                   if (value)
                   {
                       ProgramManagement.AddApp(AppView);
                   }
                   else if (!RunningStatus.Value)
                   {
                       ProgramManagement.DisableApp(AppView);
                   }
               }));

            EnableStatus.Add(new RelayAction<bool, AppEvent>(
              CLEAR_ICON_PATH,
              (value, ins) => AppStoreFile.RemoveAppIcon()));

            EnableStatus.Add(new RelayAction<bool, AppEvent>(
              SHOW_DELETE_SATAUS,
              (value, ins) => AppView.SetDeletedStatus(!value)));
        }
        private void InitActiveStatusAction()
        {
            ActiveStatus.Add(new RelayAction<bool, AppEvent>(
              ENABLE,
              (value, ins) => !value && !RunningStatus.Value,
              (value, ins) =>
              {
                  AppUnit.StopUpdate();
                  ProgramManagement.RemoveApp(AppView);
              }));

            ActiveStatus.Add(new RelayAction<bool, AppEvent>(
              SHOW_DELETE_SATAUS,
              (value, ins) => AppView.SetDeletedStatus(!value)));

        }
    }
}
