using System;
using UiStore.Services;
using UiStore.ViewModel;

namespace UiStore.Models
{
    internal class InstanceWarehouse
    {
        private AppUnit _appUnit;
        public AppUnit AppUnit { get => _appUnit; set => _appUnit = value; }

        private AppStatusInfo _appStatusInfo;
        public AppStatusInfo AppStatusInfo { get => _appStatusInfo; set => _appStatusInfo = value; }

        private AppInfoModel _appInfoModel;
        public AppInfoModel AppInfoModel { get => _appInfoModel; set => _appInfoModel = value; }

        private AppViewModel _appView;
        public AppViewModel AppViewModel { get => _appView; set => _appView = value; }

        private ProgramManagement _programManage;
        public ProgramManagement ProgramManagement { get => _programManage; set => _programManage = value; }

        private AppStoreFileManagement _appStoreFile;
        public AppStoreFileManagement AppStoreFileManagement { get => _appStoreFile; set => _appStoreFile = value; }

        private AppModelManagement _appModelManage;
        public AppModelManagement AppModelManagement { get => _appModelManage; set => _appModelManage = value; }

        public InstanceWarehouse() { }

    }
}
