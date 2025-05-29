
using System.Collections.Generic;
using UiStore.Common;
using UiStore.Models;

namespace UiStore.Configs
{
    public class ConfigModel
    {
        public ConfigModel()
        {
            // Thông tin cài đặt của chương trình
            SftpConfig = new SftpConfig() { 
                Host = "200.166.2.201",
                Port = 4422,
                User = "user",
                Password = "ubnt",
            };
            Location = new Location()
            {
                Product = "UTPG3TM0T01",
                Station = "FT6"
            };
            LaunchPassword = Util.GetMD5HashFromString("Foxconn168!!");
            RemotePath = "/AutoDownload";
            AppLocalPath = "./Apps";
            CommonLocalPath = "./Common";
            CleaningApp = true;
            UpdateTime = 20;
            Accounts.Add("V3233228", Util.GetMD5HashFromString("Foxconn123!!"));
        }
        public string LaunchPassword { get; set; }
        public SftpConfig SftpConfig { get;  set; }

        public Location Location { get; set; }

        public bool CleaningApp { get;  set; }

        public string RemotePath { get;  set; }

        public string AppLocalPath { get;  set; }
        public string CommonLocalPath { get;  set; }
        public int UpdateTime { get; set; }
        public Dictionary<string, string> Accounts { get; private set; } = new Dictionary<string, string>();
    }
}
