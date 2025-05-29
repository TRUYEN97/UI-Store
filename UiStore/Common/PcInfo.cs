using System;
using System.Collections.Generic;
using System.Net;

namespace UiStore.Common
{
    public class PcInfo
    {
        public static string PcName
        {
            get
            {
                return Environment.MachineName.ToUpper();
            }
        }

        public static List<string> GetIps
        {
            get
            {
                var list = new List<string>();
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    // Lọc chỉ IPv4 và không phải địa chỉ loopback
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        list.Add(ip.ToString());
                    }
                }
                return list;
            }
        }
    }
}
