using System;

using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

using System.Data.Common;
using System.Linq;

namespace ReportingService.Models
{
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class SupportedEventProcessorContext : DbContext
    {
        public DbSet<ReportingService.Models.SupportedEventProcessor> SupportedEventProcessors { get; set; }

        public SupportedEventProcessorContext()
        {
        }

        // Constructor to use on a DbConnection that is already opened
        public SupportedEventProcessorContext(DbConnection existingConnection, bool contextOwnsConnection)
          : base(existingConnection, contextOwnsConnection)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SupportedEventProcessor>()
                 .HasKey(i => i.__eventId);
        }
    }
}
