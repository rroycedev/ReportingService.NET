using System;
using System.Net;
using System.Net.Sockets;

namespace ReportingServiceDatabase.Host
{
    public class HostUtil
    {
        public HostUtil()
        {
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


        public static String HostnameToIp(String hostname)
        {
            IPHostEntry hostEntry;

            hostEntry = Dns.GetHostEntry(hostname);

            //you might get more than one ip for a hostname since 
            //DNS supports more than one record

            if (hostEntry.AddressList.Length > 0)
            {
                var ip = hostEntry.AddressList[0];

                return ip.ToString();
            }

            return "";
        }

    }
}
