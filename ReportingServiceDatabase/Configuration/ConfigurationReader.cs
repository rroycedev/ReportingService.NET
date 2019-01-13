using System;

using System.Configuration;

namespace ReportingServiceDatabase.Configuration
{
    public class ConfigurationReader
    {
        public ConfigurationReader()
        {
        }

        public static String GetAppSetting(String key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
