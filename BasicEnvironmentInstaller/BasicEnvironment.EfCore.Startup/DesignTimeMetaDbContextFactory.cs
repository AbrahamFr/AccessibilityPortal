using BasicEnvironment.EfCore.SchedulingMeta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BasicEnvironment.EfCore.Startup
{
    class DesignTimeMetaDbContextFactory : IDesignTimeDbContextFactory<SchedulingMetaDbContext>
    {
        internal const string debugConnectionString = @"Server=localhost;Database=CS_Dev_meta;User Id=RockStarAdmin;Password=Hisoftware1;";
        public SchedulingMetaDbContext CreateDbContext(string[] args)
        {
            return new SchedulingMetaDbContext(SchedulingMetaDbContext.CreateConfiguration(debugConnectionString));
        }
    }
}
