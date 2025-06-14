using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UiStore.Common;

namespace UiStore.Services
{
    internal class TranforUtil
    {
        public static bool IsConnected()
        {
            using (var sftp = Util.GetSftpInstance())
            {
                return sftp.IsConnected;
            }
        }
        public static async Task<T> GetModelConfig<T>(string path, string zipPassword)
        {
            using (var sftp = Util.GetSftpInstance())
            {
                if (!await sftp.Connect())
                {
                    string errorStr = "Connect to server failded!";
                    throw new ConnectFaildedException(errorStr);
                }

                if (!await sftp.Exists(path))
                {
                    string errorStr = $"Station invalid!";
                    throw new SftpFileNotFoundException(errorStr);
                }

                try
                {
                    string appConfig = await sftp.DownloadZipFileFormModel(path, zipPassword);
                    if (string.IsNullOrEmpty(appConfig))
                    {
                        return default;
                    }
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
