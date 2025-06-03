using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string ProgramFolderPath { get; set; }
        public string CommonFolderPath { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
    }
}
