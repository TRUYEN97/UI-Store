using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;
using static UiStore.Common.ConstKey;

namespace UiStore.Services
{
    internal class AppModelManagement
    {
        private readonly AppUnit _appUnit;
        private readonly AppStatusInfo _appStatus;
        private readonly ProgramManagement _programManagement;
        private AppModel appModel;
        public AppModel CurrentAppModel { get; private set; }
        public AppModel AppModelUse { get; set; }
        public HashSet<FileModel> ToRemoveFiles {  get; private set; }

        public AppModelManagement(AppUnit appUnit)
        {
            _appUnit = appUnit;
            _appStatus = appUnit.AppStatusInfo;
            _programManagement = appUnit.ProgramManagement;
        }

        internal async Task<bool> UpdateAppModel()
        {
            string appPath = _appUnit?.AppInfoModel?.AppPath;
            if (string.IsNullOrWhiteSpace(appPath))
            {
                _programManagement.RemoveApp(_appUnit.AppView, CurrentAppModel);
                return false;
            }
            appModel = await TranforUtil.GetModelConfig<AppModel>(appPath, ZIP_PASSWORD);
            if (appModel == null)
            {
                _programManagement.RemoveApp(_appUnit.AppView, CurrentAppModel);
                _appStatus.IsAppAvailable = false;
                return false;
            }
            CompareModel(appModel);
            if (InitStorePathFor(appModel))
            {
                _appStatus.IsAppAvailable = true;
                _appStatus.IsEnable = appModel.Enable && appModel.FileModels != null && appModel.FileModels.Count > 0;
                _appStatus.IsCloseAndClear = appModel.CloseAndClear;
                _appStatus.IsAutoRun = appModel.AutoOpen;
                CurrentAppModel = appModel;
                return true;
            }
            return false;
        }

        private void CompareModel(AppModel appModel)
        {
            bool rs = false;
            if (!AppModelComparetor.CompareInfo(CurrentAppModel, appModel))
            {
                rs = true;
            }
            ToRemoveFiles = AppModelComparetor.CompareFiles(AppModelUse, appModel);
            _appStatus.HasNewVersion = rs || ToRemoveFiles.Count > 0;
        }

        private bool InitStorePathFor(AppModel appModel)
        {
            string rootFolderPath = _appUnit?.AppInfoModel.ProgramFolderPath;
            if (appModel == null || string.IsNullOrWhiteSpace(rootFolderPath))
            {
                return false;
            }
            foreach (var file in appModel.FileModels)
            {
                if(Util.ArePathsEqual(file.ProgramPath, appModel.MainPath))
                {
                    appModel.AppIconPath = Path.Combine(_appUnit.AppInfoModel.IconDir, file.ProgramPath);
                    appModel.AppIconFileModel = file;
                }
                string storeFile = file.ProgramPath;
                if (!string.IsNullOrWhiteSpace(rootFolderPath))
                {
                    storeFile = Path.Combine(rootFolderPath, file.ProgramPath);
                }
                file.StorePath = storeFile;
            }
            return true;
        }
    }
}
