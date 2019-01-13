using System;

using System.Configuration;

namespace ReportingServiceDaemon
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);

            Listener.Start(port);
        }
    }
}
