using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UiStore.Configs;

namespace UiStore.Models
{
    internal class AppInfoModel : ProgramPathModel
    {
        public AppInfoModel(ProgramPathModel programPathModel) { 
            Update(programPathModel);
        }

        public AppInfoModel()
        {
        }

        public void Update(ProgramPathModel programPathModel)
        {
            AppPath = programPathModel.AppPath;
            AccectUserPath = programPathModel.AccectUserPath;
        }
        public string RootDir { get; set; }
        public string ProgramFolderPath => Path.Combine(RootDir, Name);
        public string CommonFolderPath { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string IconDir => Path.Combine(RootDir, "Icons", Name);
    }
}
