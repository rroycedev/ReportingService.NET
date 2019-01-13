using System;

using ReportingServiceNetwork.Sockets;

namespace ReportingServiceDaemon
{
    public static class Listener
    {
        public static void Start(int port)
        {
            try
            {
                SynchronousSocketListener listener = new SynchronousSocketListener
                {
                    HandleRequest = Listener.HandleRequest
                };

                listener.StartListening(port);
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
