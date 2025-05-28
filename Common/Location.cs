using System;

namespace UiStore.Common
{
    public class Location
    {
        public string PcName
        {
            get
            {
                return PcInfo.PcName;
            }
        }
        public string Product { get; set; }
        public string Station { get; set; }
        public string PcNumber
        {
            get
            {
                string pcName = PcInfo.PcName.Trim();
                if (pcName.Contains("-"))
                {
                    return pcName.Substring(pcName.LastIndexOf('-') + 1);
                }
                return "00";
            }
        }

        public Location()
        {
        }

    }
}
