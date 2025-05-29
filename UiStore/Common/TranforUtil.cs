using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UiStore.Model;

namespace UiStore.Common
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
                    string errorStr = "Mất kết nối với server!";
                    throw new Exception(errorStr);
                }

                if (!await sftp.Exists(path))
                {
                    string errorStr = $"Trạm test này chưa được cài đặt chương trình!";
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
                    string errorStr = $"File appConfig.json tại: {path} có thể bị lỗi format.\r\nHãy kiểm tra thủ công để tránh mất dữ liệu!";
                    throw new Exception(errorStr, ex);
                }
            }
        }


    }
}
