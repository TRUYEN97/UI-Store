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
    }
}
