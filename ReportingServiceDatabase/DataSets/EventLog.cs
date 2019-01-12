using System;


namespace ReportingServiceDatabase.DataSets
{
    public class EventLog
    {
        public ulong EventLogId { get; set; }
        public ulong EventId { get; set; }
        public String EventDate { get; set; }
        public String LogMessage { get; set; }
    }
}
