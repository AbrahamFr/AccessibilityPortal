using ComplianceSheriff.CheckpointGroups;
using ComplianceSheriff.DataFormatter;
using ComplianceSheriff.Passwords;
using ComplianceSheriff.UserAccounts;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ComplianceSheriffDomainServices
    {

        public static IServiceCollection AddComplianceSheriffDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<ComplianceSheriff.Work.IUnitOfWorkFactory, ComplianceSheriff.Work.UnitOfWorkFactory>();
            services.AddScoped<ICheckpointGroupService, CheckpointGroupService>();
            services.AddScoped<IUserAccountManagerService, UserAccountManagerService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IDataFormatterService, DataFormatterService>();

            return services;
        }
    }
}
