using System;

using ReportingServiceNetwork.Sockets;

namespace ReportingServiceDaemon
{
    public class Listener
    {
        public static void Start(int port)
        {
            try
            {
                SynchronousSocketListener.StartListening(port, HandleRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return;
            }
        }

        public static  void HandleRequest(ReportingServiceNetwork.Messages.ReportingServiceMessage msg)
        {

        }
    }
}
