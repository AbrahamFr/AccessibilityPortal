From the root of this project

To make a new migration:

    dotnet ef migrations add <MigrationName> --context BasicEnvironment.EfCore.SchedulingMeta.SchedulingMetaDbContext --startup-project ..\BasicEnvironment.EfCore.Startup


To remove a migration:

    dotnet ef migrations remove --context BasicEnvironment.EfCore.SchedulingMeta.SchedulingMetaDbContext --startup-project ..\BasicEnvironment.EfCore.Startup


To run a migration:

    dotnet ef database update <MigrationName> --context BasicEnvironment.EfCore.SchedulingMeta.SchedulingMetaDbContext --startup-project ..\BasicEnvironment.EfCore.Startup
