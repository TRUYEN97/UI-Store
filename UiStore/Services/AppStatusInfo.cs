using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiStore.Services
{
    internal class AppStatusInfo
    {
        private readonly AppEvent appEvent;
        public AppStatusInfo(AppEvent appEvent)
        {
            this.appEvent = appEvent;
        }

        public bool IsRunning
        {
            get => appEvent.RunningStatus.Value.Item1;
            set => appEvent.RunningStatus.Value = (value,  IsCloseAndClear);
        }
        public bool IsAutoRun
        {
            get => appEvent.AutoRunAction.Value;
            set => appEvent.AutoRunAction.Value = value;
        }
        public UpdateState UpdateStatus
        {
            get => appEvent.UpdateAction.Value;
            set => appEvent.UpdateAction.Value = value;
        }
        public ExtractState ExtractStatus
        {
            get => appEvent.ExtractAction.Value;
            set => appEvent.ExtractAction.Value = value;
        }
        public bool HasNewVersion
        {
            get => appEvent.NewVersionStatus.Value;
            set => appEvent.NewVersionStatus.Value = value;
        }

        public bool IsAppAvailable { get => this.appEvent.ActiveStatus.Value; set { this.appEvent.ActiveStatus.Value = value; } }
        public bool IsEnable { get => this.appEvent.EnableStatus.Value; set { this.appEvent.EnableStatus.Value = value; } }
        public bool IsCloseAndClear { get; set; }
        public bool IsRunnable => !IsRunning && UpdateStatus == UpdateState.SUCCESS && ExtractStatus == ExtractState.SUCCESS && !HasNewVersion;
        public bool IsExtractable => UpdateStatus == UpdateState.SUCCESS && ExtractStatus != ExtractState.EXTRACTING && !HasNewVersion;
        public bool IsUpdateAble => !IsRunning && UpdateStatus != UpdateState.UPDATING && ExtractStatus != ExtractState.EXTRACTING && !HasNewVersion;

       
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
            EXTRACTING= 1,
            FAILED = 2
        }
    }
}
