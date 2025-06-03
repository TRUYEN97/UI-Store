using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiStore.Common
{
    public static class ConstKey
    {
        public static readonly string ZIP_PASSWORD = Util.GetMD5HashFromString("@RaspberryPi5@");
        public static class DoStatus
        {
            public const int DO_NOTHING = 0;
            public const int CHECK_UPDATE_STATE = 1;
            public const int CREATE_STATE = 2;
        }
        public static class AppStatus
        {
            public const int STANDBY = 0;
            public const int HAS_NEW_VERSION = 1;
            public const int DELETED = 2;
            public const int UPDATE_FAILED = 3;
        }
    }
}
