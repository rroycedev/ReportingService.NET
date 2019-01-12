using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportingServiceDatabase.DataSets
{
    public partial class ReportEvent 
    {
        public ulong Id { get; set; }
        public ulong EventId { get; set; }
        public String EventData { get; set; }
        public String EventDate { get; set; }
    }
}
