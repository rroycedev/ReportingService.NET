using System;
namespace ReportingServiceNetwork.Sockets
{

    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    using ReportingServiceNetwork.Messages;

    public static class SynchronousSocketListener
    {

        // Incoming data from the client.  
        public static string data = null;

        public static void StartListening(int port)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            listener.Bind(localEndPoint);
            listener.Listen(10);

            string sEOT = "\\x04";

            byte[] buffer = null;

            // Start listening for connections.  
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();
                data = null;

                // An incoming connection needs to be processed.  
                while (true)
                {
                    int bytesRec = handler.Receive(bytes);

                    if (bytesRec == 0)
                    {
                        break;
                    }

                    Console.WriteLine("Received " + bytesRec + " bytes....");

                    if (buffer != null)
                    {
                        byte[] newbuffer = new byte[buffer.Length + bytesRec];

                        Buffer.BlockCopy(buffer, 0, newbuffer, 0, buffer.Length);
                        Buffer.BlockCopy(bytes, 0, newbuffer, buffer.Length, bytesRec);

                        buffer = null;

                        buffer = newbuffer;
                    }
                    else
                    {
                        buffer = new byte[bytesRec];

                        Buffer.BlockCopy(bytes, 0, buffer, 0, bytesRec);
                    }

                    string thisData = Encoding.ASCII.GetString(buffer);

                    if (thisData.Contains(sEOT))
                    {
                        break;
                    }
                }

                string receivedData = Encoding.ASCII.GetString(buffer);

                string resultMsg = "";

                if (!receivedData.StartsWith(ReportingServiceMessage.SOH, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Invalid message");
                    resultMsg = "Invalid Message";
                }
                else if (!receivedData.EndsWith(ReportingServiceMessage.EOT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Invalid message");
                    resultMsg = "Invalid Message";
                }
                else
                {
                    int pos = receivedData.IndexOf(ReportingServiceMessage.STX, StringComparison.CurrentCulture);

                    int endpos = receivedData.IndexOf(ReportingServiceMessage.ETX, pos, StringComparison.CurrentCulture);

                    byte[] bMsgType = new byte[endpos - pos - Encoding.ASCII.GetBytes(ReportingServiceMessage.STX).Length];

                    Buffer.BlockCopy(buffer, pos + Encoding.ASCII.GetBytes(ReportingServiceMessage.STX).Length, bMsgType, 0, endpos - pos - Encoding.ASCII.GetBytes(ReportingServiceMessage.STX).Length);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(bMsgType);

                    int msgType = BitConverter.ToInt32(bMsgType, 0);


                    Console.WriteLine("Msg Type {0}", msgType);

                    System.Collections.Generic.List<string> arguments = new System.Collections.Generic.List<string>();

                    while (true)
                    {

                        pos = receivedData.IndexOf(ReportingServiceMessage.STX, endpos, StringComparison.CurrentCulture);

                        if (pos == -1)
                        {
                            break;
                        }

                        endpos = receivedData.IndexOf(ReportingServiceMessage.ETX, pos, StringComparison.CurrentCulture);

                        byte[] bArg = new byte[endpos - pos - Encoding.ASCII.GetBytes(ReportingServiceMessage.STX).Length];

                        Buffer.BlockCopy(buffer, pos + Encoding.ASCII.GetBytes(ReportingServiceMessage.STX).Length, bArg, 0, endpos - pos - Encoding.ASCII.GetBytes(ReportingServiceMessage.STX).Length);

                        string sArg = Encoding.ASCII.GetString(bArg);

                        arguments.Add(sArg);
                    }

                    ReportingServiceMessage receivedMsg = new ReportingServiceMessage();

                    receivedMsg.MessageType = (ReportingServiceMessage.MessageTypes)msgType;
                    receivedMsg.Arguments = arguments.ToArray();

                    resultMsg = "Ok";
                }

                // Show the data on the console.  

                Console.WriteLine("Text received : {0}", receivedData);

                // Echo the data back to the client.  
                byte[] msg = Encoding.ASCII.GetBytes(resultMsg);

                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }

    }

}
