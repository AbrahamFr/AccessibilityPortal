using Microsoft.EntityFrameworkCore;
using System;

namespace BasicEnvironment.EfCore.SchedulingMeta
{
    public class SchedulingMetaDbContext : DbContext
    {
        // Overriding construtor for dependency injection support
        public SchedulingMetaDbContext(SchedulingMetaConfiguration config) : base(config.Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Not creating data model at this time.
            // To be done in the future for actual Entity Framework support.
        }

        public static SchedulingMetaConfiguration CreateConfiguration(string debugConnectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(debugConnectionString, b => b.MigrationsAssembly(typeof(SchedulingMetaDbContext).Assembly.FullName));
            return new SchedulingMetaConfiguration { Options = optionsBuilder.Options };
        }
    }
}
