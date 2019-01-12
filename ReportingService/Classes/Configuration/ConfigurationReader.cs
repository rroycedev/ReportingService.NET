using System;

using System.Configuration;

namespace ReportingService.Classes.Configuration
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
