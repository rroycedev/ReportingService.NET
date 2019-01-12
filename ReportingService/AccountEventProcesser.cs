using System;

using ReportingReflectionLibrary;
using ReportingServiceDatabase.Logging;

namespace AccountEventHandler
{
    public  class AccountEventProcesser : EventProcesser
    {
        public AccountEventProcesser()
        {

        }

        public override bool Start()
        {
            Logger.Debug("AccountEventProcesser: Starting");

            return true;
        }
    }
}
