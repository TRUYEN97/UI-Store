using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UiStore.Common;
using UiStore.Models;
using static UiStore.Common.ConstKey;

namespace UiStore.Services
{
    internal class AppModelManagement
    {
        private readonly AppInfoModel _appInfoModel;
        private readonly AppStatusInfo _appStatus;
        private AppModel AppModel { get; set; }
        private AppModel AppModelUse { get; set; }
        public HashSet<FileModel> ToRemoveFiles { get; set; }
        public AppModel CurrentAppModel { get; set; }
        public AppModelManagement(AppInfoModel appInfoModel, AppStatusInfo appStatusInfo)
        {
            _appInfoModel = appInfoModel;
            _appStatus = appStatusInfo;
        }

        internal async Task<AppModel> GetAppModel()
        {
            string appPath = _appInfoModel?.AppPath;
            if (string.IsNullOrWhiteSpace(appPath))
            {
                _appStatus.IsAppAvailable = false;
                return null;
            }
            AppModel = await TranforUtil.GetModelConfig<AppModel>(appPath, ZIP_PASSWORD);
            if (AppModel == null)
            {
                _appStatus.IsAppAvailable = false;
                return null;
            }
            if (InitStorePathFor(AppModel))
            {
                _appStatus.IsAppAvailable = true;
                _appStatus.IsEnable = AppModel.Enable && AppModel.FileModels != null && AppModel.FileModels.Count > 0;
                _appStatus.IsCloseAndClear = AppModel.CloseAndClear;
                _appStatus.IsAutoRun = AppModel.AutoOpen;
                return AppModel;
            }
            return null;
        }

        internal bool IsModelChanged(AppModel appModel)
        {
            bool rs = false;
            if (!AppModelComparetor.CompareInfo(CurrentAppModel, appModel))
            {
                rs = true;
            }
            ToRemoveFiles = AppModelComparetor.CompareFiles(AppModelUse, appModel);
            return rs || ToRemoveFiles.Count > 0;
        }

        private bool InitStorePathFor(AppModel appModel)
        {
            string rootFolderPath = _appInfoModel.ProgramFolderPath;
            string CommonFolderPath = _appInfoModel.CommonFolderPath;
            if (appModel == null || string.IsNullOrWhiteSpace(rootFolderPath))
            {
                return false;
            }
            foreach (var file in appModel.FileModels)
            {
                if (Util.ArePathsEqual(file.ProgramPath, appModel.MainPath))
                {
                    appModel.AppIconPath = Path.Combine(_appInfoModel.IconDir, file.ProgramPath);
                    appModel.AppIconFileModel = file;
                }
                string storeFile = file.ProgramPath;
                if (!string.IsNullOrWhiteSpace(rootFolderPath))
                {
                    storeFile = Path.Combine(rootFolderPath, file.ProgramPath);
                }
                file.ZipPath = Path.Combine(CommonFolderPath, Path.GetFileName(file.RemotePath));
                file.StorePath = storeFile;
            }
            return true;
        }

        internal void UpdateCurrentModel()
        {
            CurrentAppModel = AppModel;
        }
        internal void UpdateUseModel()
        {
            AppModelUse = CurrentAppModel;
        }
    }
}
