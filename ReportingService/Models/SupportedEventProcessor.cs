using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace ReportingService.Models
{
    [Table("event_processors")]
    public class SupportedEventProcessor
    {
        [Key]
        [Column("event_id", Order = 1)]
        public byte[] __eventId { get; set; }

        public ulong EventId
        {
            get
            {
                return 1;
            }

        }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("assembly_filename")]
        public string AssemblyFilename { get; set; }

        [Column("class_name")]
        public string ClassName { get; set; }

        [Column("method_name")]
        public string MethodName { get; set; }
    }
}
