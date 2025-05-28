using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UiStore.Common
{
    public static class ConstKey
    {
        public static string ZIP_PASSWORD = "Foxconn168!!";
        public static class AppState
        {
            public static readonly int UPDATE_STATE = 1;
            public static readonly int CREATE_STATE = 2;
            public static readonly int STANDBY_STATE = 0;
        }
    }
}
