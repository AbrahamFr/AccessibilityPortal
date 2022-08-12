using Microsoft.EntityFrameworkCore;
using System;

namespace BasicEnvironment.EfCore.SchedulingMain
{
    public class SchedulingMainDbContext : DbContext
    {
        // Overriding construtor for dependency injection support
        public SchedulingMainDbContext(SchedulingMainConfiguration config) : base(config.Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Not creating data model at this time.
            // To be done in the future for actual Entity Framework support.
        }

        public static SchedulingMainConfiguration CreateConfiguration(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SchedulingMainDbContext).Assembly.FullName));
            return new SchedulingMainConfiguration { Options = optionsBuilder.Options };
        }
    }
}
