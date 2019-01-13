using System;

using ReportingServiceNetwork.TUSocket;
using ReportingServiceNetwork.Messages;

namespace SocketTest
{
    class MainClass
    {
        public static void Syntax()
        {
            Console.WriteLine("Syntax: SocketTest.exe <test_type>");
            Console.WriteLine("        Where <test_type> is 'server' or 'client'");
        }

        public static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                Syntax();
                return;
            }

            string testType = args[0].ToUpper();

            if (testType != "SERVER" && testType != "CLIENT") 
            {
                Syntax();
                return;
            }

            switch (testType)
            {
                case "SERVER":
                    try
                    {
                        SynchronousSocketListener.StartListening(11000);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        return;
                    }
                    break;
                case "CLIENT":
                    try
                    {
                        string[] msgArgs = new string[2];

                        msgArgs[0] = "Argument 1";
                        msgArgs[1] = "Argument 2";

                        ReportingServiceMessage msg = new ReportingServiceMessage
                        {
                            MessageType = ReportingServiceMessage.MessageTypes.REGISTER,
                            Arguments = msgArgs
                        };

                        SynchronousSocketClient.StartClient(msg, 11000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        return;
                    }
                    break;
            }

        }
    }
}
