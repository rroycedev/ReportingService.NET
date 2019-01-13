using System;

using ReportingServiceNetwork.TUSocket;

namespace ReportingServiceDaemon
{
    public static class Listener
    {
        public static void Start(int port)
        {
            try
            {
                SynchronousSocketListener.StartListening(port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return;
            }
        }
    }
}
