using ComplianceSheriff.AdoNet;
using ComplianceSheriff.AdoNet.AuditReports;
using ComplianceSheriff.AdoNet.Authentication;
using ComplianceSheriff.AdoNet.Checkpoints;
using ComplianceSheriff.AdoNet.ScanGroups;
using ComplianceSheriff.AdoNet.Scans;
using ComplianceSheriff.AuditReports;
using ComplianceSheriff.Authentication;
using ComplianceSheriff.Checkpoints;
using ComplianceSheriff.ScanGroups;
using ComplianceSheriff.Scans;
using Microsoft.Extensions.Configuration;
using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.AdoNet.UsageAudit;
using ComplianceSheriff.Permission;
using ComplianceSheriff.AdoNet.Permission;
using ComplianceSheriff.AdoNet.CheckpointGroups;
using ComplianceSheriff.CheckpointGroups;
using ComplianceSheriff.LogMessages;
using ComplianceSheriff.AdoNet.LogMessages;
using ComplianceSheriff.AdoNet.Runs;
using ComplianceSheriff;
using ComplianceSheriff.UserGroups;
using ComplianceSheriff.AdoNet.UserGroups;
using ComplianceSheriff.ApiRoles;
using ComplianceSheriff.AdoNet.ApiRoles;
using ComplianceSheriff.UserAgent;
using ComplianceSheriff.AdoNet.UserAgent;
using ComplianceSheriff.IssueTrackerReport;
using ComplianceSheriff.AdoNet.IssueTracker;
using ComplianceSheriff.Users;
using ComplianceSheriff.AdoNet.Users;
using ComplianceSheriff.UserMapping;
using ComplianceSheriff.AdoNet.UserMapping;
using ComplianceSheriff.UserInfos;
using ComplianceSheriff.AdoNet.UserInfos;
using ComplianceSheriff.ScanGroupScans;
using ComplianceSheriff.AdoNet.ScanGroupScans;
using ComplianceSheriff.ScanGroupSubGroups;
using ComplianceSheriff.AdoNet.ScanGroupSubGroups;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ComplianceSheriffAdoNetServices
    {
        public static IServiceCollection AddComplianceSheriffAdoNet(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IConnectionManager>(sp => new ConnectionManager(sp.GetRequiredService<IConnectionFactory>().GetContextDBConnection()));
            services.AddScoped<AdoNetTransactionLifecycle>();
            services.AddScoped<IScanGroupAccessor, ScanGroupAccessor>();
            services.AddScoped<IScanMutator, ScanMutator>();
            services.AddScoped<IScanAccessor, ScanAccessor>();
            services.AddScoped<IAuthAccessor, AuthAccessor>();
            services.AddScoped<IScanGroupScanAccessor, ScanGroupScanAccessor>();
            services.AddScoped<ICheckpointAccessor, CheckpointAccessor>();
            services.AddScoped<IIssuesAccessor, IssuesAccessor>();
            services.AddScoped<IAuditReportMutator, AuditReportMutator>();
            services.AddScoped<IAuditReportAccessor, AuditReportAccessor>();
            services.AddScoped<IUsageAuditMutator, UsageAuditMutator>();
            services.AddScoped<IPermissionAccessor, PermissionAccessor>();
            services.AddScoped<IPermissionMutator, PermissionMutator>();
            services.AddScoped<ILogMessagesMutator, LogMessagesMutator>();
            services.AddScoped<IRunAccessor, RunAccessor>();
            services.AddScoped<ICheckpointGroupsAccessor, CheckpointGroupsAccessor>();
            services.AddScoped<IUserGroupAccessor, UserGroupAccessor>();
            services.AddScoped<IApiRoleAccessor, ApiRolesAccessor>();
            services.AddScoped<IUserAgentAccessor, UserAgentAccessor>();
            services.AddScoped<IIssueTrackerAccessor, IssueTrackerAccessor>();
            services.AddScoped<IScanAccessor, ScanAccessor>();
            services.AddScoped<ILogMessagesAccessor, LogMessagesAccessor>();
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IUserMutator, UserMutator>();
            services.AddScoped<IUserGroupMutator, UserGroupMutator>();
            services.AddScoped<IUserGroupAccessor, UserGroupAccessor>();
            services.AddScoped<IUserMapperAccessor, UserMapperAccessor>();
            services.AddScoped<IUserInfoAccessor, UserInfoAccessor>();
            services.AddScoped<IUserInfoMutator, UserInfoMutator>();
            services.AddScoped<IScanGroupMutator, ScanGroupMutator>();
            services.AddScoped<IScanGroupAccessor, ScanGroupAccessor>();
            services.AddScoped<IScanGroupScansAccessor, ScanGroupScansAccessor>();
            services.AddScoped<IScanGroupScansMutator, ScanGroupScansMutator>();
            services.AddScoped<IScanGroupSubGroupAccessor, ScanGroupSubGroupAccessor>();

            return services;
        }
    }
}
