using System;
using System.IO;
using UiStore.Configs;

namespace UiStore.Common
{
    internal class PathUtil
    {

        internal static string GetRemotePath()
        {
            return AutoDLConfig.ConfigModel.RemotePath;
        }

        internal static string GetProductPath(Location location)
        {
            return Path.Combine(GetRemotePath(), location.Product);
        }

        internal static string GetStationPath(Location location)
        {
            return Path.Combine(GetProductPath(location), location.Station);
        }

        internal static string GetProgramFolderPath(Location location)
        {
            return Path.Combine(GetStationPath(location), "Program");
        }

        internal static string GetStationAccessUserPath(Location location)
        {
            return Path.Combine(GetStationPath(location), "AccessUserList.zip");
        }

        internal static string GetAppConfigRemotePath(Location location)
        {
            return Path.Combine(GetStationPath(location), "Apps.zip");
        }
    }
}
