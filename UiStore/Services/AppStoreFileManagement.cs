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

        public bool RemoveProgramFolder()
        {
            try
            {
                string dir = appInfoModel?.ProgramFolderPath;
                if (!string.IsNullOrEmpty(dir))
                {
                    if (Directory.Exists(dir))
                    {
                        Directory.Delete(dir, true);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                RemoveAppIcon();
            }
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

        internal bool CleanStoreFileModel()
        {
            try
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
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
