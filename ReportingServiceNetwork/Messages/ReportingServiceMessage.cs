using System;
using System.Text;

namespace ReportingServiceNetwork.Messages
{
    public class ReportingServiceMessage
    {
        public enum MessageTypes {
            REGISTER = 1,
            DEGREGISTER = 2
        };

        public static string SOH = "\\x01";
        public static string STX = "\\x02";
        public static string ETX = "\\x03";
        public static string EOT = "\\x04";        

        public MessageTypes MessageType { get; set; }

        public string[] Arguments { get; set; }

        public byte[] SocketData
        {
            get
            {
                string argumentData = "";

                byte[] bSOH = Encoding.ASCII.GetBytes(SOH);
                byte[] bSTX = Encoding.ASCII.GetBytes(STX);
                byte[] bETX = Encoding.ASCII.GetBytes(ETX);
                byte[] bEOT = Encoding.ASCII.GetBytes(EOT);

                byte[] intBytes = BitConverter.GetBytes((int)this.MessageType);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);

                byte[] bMsgType = intBytes;

                int bytesNeeded = bSOH.Length + bMsgType.Length + bSTX.Length + bETX.Length + bEOT.Length;

                if (this.Arguments != null && this.Arguments.Length > 0)
                {
                    foreach (string arg in this.Arguments)
                    {
                        argumentData += String.Format("{0}{1}{2}", STX, arg, ETX);
                    }

                }

                byte[] bArgumentData = Encoding.ASCII.GetBytes(argumentData);

                bytesNeeded += bArgumentData.Length;

                byte[] msgBytes = new byte[bytesNeeded];

                Buffer.BlockCopy(bSOH, 0, msgBytes, 0, bSOH.Length);

                int pos = bSOH.Length;

                Buffer.BlockCopy(bSTX, 0, msgBytes, pos, bSTX.Length);

                pos += bSTX.Length;

                Buffer.BlockCopy(bMsgType, 0, msgBytes, pos, bMsgType.Length);

                pos += bMsgType.Length;

                Buffer.BlockCopy(bETX, 0, msgBytes, pos, bETX.Length);

                pos += bETX.Length;

                Buffer.BlockCopy(bArgumentData, 0, msgBytes, pos, bArgumentData.Length);

                pos += bArgumentData.Length;

                Buffer.BlockCopy(bEOT, 0, msgBytes, pos, bEOT.Length);

                return msgBytes;
            }
        }
    }
}
