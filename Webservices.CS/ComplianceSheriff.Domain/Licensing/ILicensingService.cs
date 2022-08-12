using ComplianceSheriff.Configuration;

namespace ComplianceSheriff.Licensing
{
    public interface ILicensingService
    {
        Allocator GetLicenseInfo(string organizationId, ConfigurationOptions configOptions);

        string GetLicensedModuleString(string organizationId, ConfigurationOptions configOptions);
    }
}
