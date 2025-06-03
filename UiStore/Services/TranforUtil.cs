using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UiStore.Common;
using UiStore.Models;

namespace UiStore.Services
{
    internal class TranforUtil
    {
        internal static async Task<(AppList, string)> GetAppListModel(Location location, string zipPassword)
        {
            string appConfigRemotePath = PathUtil.GetAppConfigRemotePath(location);
            return (await GetModelConfig<AppList>(appConfigRemotePath, zipPassword), appConfigRemotePath);
        }

        internal static async Task<T> GetModelConfig<T>(string path, string zipPassword)
        {
            using (var sftp = Util.GetSftpInstance())
            {
                if (!await sftp.Connect())
                {
                    string errorStr = "Connect to server failded!";
                    throw new Exception(errorStr);
                }

                if (!await sftp.Exists(path))
                {
                    string errorStr = $"Station invalid!";
                    throw new Exception(errorStr);
                }

                try
                {
                    string appConfig = await sftp.DownloadZipFileFormModel(path, zipPassword);
                    var result = JsonConvert.DeserializeObject<T>(appConfig);
                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception(path, ex);
                }
            }
        }


    }
}
