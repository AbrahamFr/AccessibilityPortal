using Microsoft.AspNetCore.Http;

namespace ComplianceSheriff.Urls
{
    public interface IUrlServices
    {
        string GetReferrerUrl(HttpContext context, string orgVirtualDir);

        string GetReferrerUrl(HttpContext context);

        string SanitizeUrl(string url, bool isXml = false);
    }
}
