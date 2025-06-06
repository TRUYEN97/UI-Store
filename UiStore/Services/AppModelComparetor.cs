

using System.Collections.Generic;
using UiStore.Models;

namespace UiStore.Services
{
    internal class AppModelComparetor
    {
        public static bool CompareInfo(AppModel currernt, AppModel appModel)
        {
            if (currernt == null || appModel == null)
            {
                return false;
            }
            if (currernt.OpenCmd != appModel.OpenCmd)
            {
                return false;
            }
            if (currernt.CloseCmd != appModel.CloseCmd)
            {
                return false;
            }
            if (currernt.FTUVersion != appModel.FTUVersion)
            {
                return false;
            }
            if (currernt.BOMVersion != appModel.BOMVersion)
            {
                return false;
            }
            if (currernt.FCDVersion != appModel.FCDVersion)
            {
                return false;
            }
            if (currernt.Version != appModel.Version)
            {
                return false;
            }
            if (currernt.MainPath != appModel.MainPath)
            {
                return false;
            }
            return true;
        }
        public static HashSet<FileModel> CompareFiles(AppModel currernt, AppModel appModel)
        {
            if (currernt?.FileModels == null || appModel?.FileModels == null)
            {
                return new HashSet<FileModel>();
            }
            var toRemoves = new HashSet<FileModel>(currernt.FileModels);
            toRemoves.ExceptWith(appModel.FileModels);
            return toRemoves;
        }
    }
}
