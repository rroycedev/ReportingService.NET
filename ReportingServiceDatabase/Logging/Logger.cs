using System;
namespace ReportingServiceDatabase.Logging
{
    public static class Logger
    {
        public static void Debug(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": DEBUG     " + msg);
        }

        public static void Error(string msg)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": ERROR     " + msg);
        }
    }
}
