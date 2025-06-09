using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiStore.Models;

namespace UiStore.Services
{
    internal class AppStoreFileManagement
    {
        private readonly AppInfoModel appInfoModel;
        private readonly AppModelManagement appModelManagement;

        public AppStoreFileManagement(AppInfoModel appInfoModel, AppModelManagement appModelManagement)
        {
            this.appInfoModel = appInfoModel;
            this.appModelManagement = appModelManagement;
        }

        public void RemoveProgramFolder()
        {
            string dir = appInfoModel?.ProgramFolderPath;
            if (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            RemoveAppIcon();
        }

        internal void RemoveAppIcon()
        {
            string dir = appInfoModel?.IconDir;
            if (!string.IsNullOrEmpty(dir))
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        internal void CleanStoreFileModel()
        {
            HashSet<FileModel> filesToRemove = appModelManagement.ToRemoveFiles;
            if (filesToRemove != null)
            {
                foreach (var fileModel in filesToRemove)
                {
                    if (File.Exists(fileModel.StorePath))
                    {
                        File.Delete(fileModel.StorePath);
                    }
                }
            }
        }
    }
}
