using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UiStore.Common
{
    internal class ProgramInfo
    {
        internal static string ProductVersion => Application.ProductVersion;

        internal static string CompanyName => Application.CompanyName;

        internal static string ProductName => Application.ProductName;

        internal static string CurrentCultureName => Application.CurrentCulture.Name;
    }
}
