using System;
namespace ReportingService.DataSets
{
    public class SupportedEventProcessor
    {
        public ulong EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssemblyFilename { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
    }
}
