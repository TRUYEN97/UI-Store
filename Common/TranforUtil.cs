using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UiStore.Model;

namespace UiStore.Common
{
    internal class TranforUtil
    {

        internal static async Task<AppList> GetAppListModel(Location location)
        {
            string appConfigRemotePath = PathUtil.GetAppConfigRemotePath(location);
            return await GetModelConfig<AppList>(appConfigRemotePath);
        }

        internal static async Task<T> GetModelConfig<T>(string path)
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
                    string appConfig = await sftp.ReadAllText(path);
                    var result = JsonConvert.DeserializeObject<T>(appConfig);
                    return result;
                }
                catch (Exception ex)
                {
                    string errorStr = $"File: {path}, có thể bị lỗi format.\r\nHãy kiểm tra thủ công để tránh mất dữ liệu!";
                    throw new Exception(errorStr, ex);
                }
            }
        }

    }
}
