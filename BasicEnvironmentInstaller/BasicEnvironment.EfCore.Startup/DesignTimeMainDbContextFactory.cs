using BasicEnvironment.EfCore.SchedulingMain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BasicEnvironment.EfCore.Startup
{
    class DesignTimeMainDbContextFactory : IDesignTimeDbContextFactory<SchedulingMainDbContext>
    {
        internal const string debugConnectionString = @"Server=localhost;Database=CS_Dev_main;User Id=RockStarAdmin;Password=Hisoftware1;";

        public SchedulingMainDbContext CreateDbContext(string[] args)
        {
            return new SchedulingMainDbContext(SchedulingMainDbContext.CreateConfiguration(debugConnectionString));
        }
    }
}
