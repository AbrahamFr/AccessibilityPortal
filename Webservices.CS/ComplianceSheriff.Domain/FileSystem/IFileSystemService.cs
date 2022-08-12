using ComplianceSheriff.Configuration;
using System.Threading.Tasks;

namespace ComplianceSheriff.FileSystem
{
    public interface IFileSystemService
    {
        string GetCustomerFolder(string organizationId);

        Task<string> ReadTextAsync(string filePath);
    }
}
