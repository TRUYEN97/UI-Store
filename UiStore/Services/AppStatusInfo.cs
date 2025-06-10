using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;

namespace UiStore.Services
{
    internal class AppStatusInfo
    {
        private readonly AppEvent _appEvent;
        public AppStatusInfo(AppEvent appEvent)
        {
            _appEvent = appEvent;
        }

        public bool IsRunning
        {
            get => _appEvent.RunningStatus.Value;
            set => _appEvent.RunningStatus.Value = value;
        }
        public bool IsAutoRun
        {
            get => _appEvent.AutoRunAction.Value;
            set => _appEvent.AutoRunAction.Value = value;
        }
        public UpdateState UpdateStatus
        {
            get => _appEvent.UpdateAction.Value;
            set => _appEvent.UpdateAction.Value = value;
        }
        public ExtractState ExtractStatus
        {
            get => _appEvent.ExtractAction.Value;
            set => _appEvent.ExtractAction.Value = value;
        }
        public bool HasNewVersion
        {
            get => _appEvent.HasNewVersion.Value;
            set => _appEvent.HasNewVersion.Value = value;
        }

        public int Progress
        {
            get => _appEvent.Progress.Value;
            set => _appEvent.Progress.Value = value;
        }

        public bool IsAppAvailable { get => this._appEvent.ActiveStatus.Value; set { this._appEvent.ActiveStatus.Value = value; } }
        public bool IsEnable { get => this._appEvent.EnableStatus.Value; set { this._appEvent.EnableStatus.Value = value; } }
        public bool IsCloseAndClear { get; set; }
        public bool HasUpdate { get => this._appEvent.HasUpdate.Value; set { this._appEvent.HasUpdate.Value = value; } }
        public bool IsRunnable => IsAppAvailable && IsEnable && !IsRunning && UpdateStatus == UpdateState.SUCCESS && !IsExtracting && !HasNewVersion;
        public bool IsUpdateAble => IsAppAvailable && !IsUpdating && !IsExtracting;

        public bool IsUpdating => UpdateStatus == UpdateState.UPDATING;
        public bool IsExtracting => ExtractStatus == ExtractState.EXTRACTING;

        internal void SetExtractDone()
        {
            ExtractStatus = ExtractState.SUCCESS;
        }
        internal void SetExtracting()
        {
            ExtractStatus = ExtractState.EXTRACTING;
        }
        internal void SetExtractFailed()
        {
            ExtractStatus = ExtractState.FAILED;
        }

        internal void SetUpdateDone()
        {
            UpdateStatus = UpdateState.SUCCESS;
        }

        internal void SetUpdateFailed()
        {
            UpdateStatus = UpdateState.FAILED;
        }
        internal void SetUpdating()
        {
            UpdateStatus = UpdateState.UPDATING;
        }

        public enum UpdateState
        {
            SUCCESS = 0,
            UPDATING = 1,
            FAILED = 2
        }
        public enum ExtractState
        {
            SUCCESS = 0,
            EXTRACTING = 1,
            FAILED = 2
        }

    }
}
