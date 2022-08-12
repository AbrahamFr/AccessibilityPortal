using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ComplianceSheriff.Urls
{
    public class UrlServices : IUrlServices
    {
        private readonly ILogger<UrlServices> _logger;

        public UrlServices(ILogger<UrlServices> logger)
        {
            _logger = logger;
        }

        public string GetReferrerUrl(HttpContext context, string orgVirtualDir)
        {
            var request = context.Request;

            var url = string.Format("{0}://{1}/{2}", request.IsHttps ? "https" : "http", request.Host.Host, orgVirtualDir);
            return url;
        }

        public string GetReferrerUrl(HttpContext context)
        {
            var request = context.Request;

            var url = string.Format("{0}://{1}{2}", request.IsHttps ? "https" : "http", request.Host.Host, request.PathBase);
            return url;
        }

        public string SanitizeUrl(string url, bool isXml = false)
        { 
            var decodedUrl = System.Web.HttpUtility.HtmlDecode(url);

            //if (isXml)
            //{
            //    return string.Format("<![CDATA[{0}]])>", decodedUrl);
            //    //return decodedUrl.Replace("&", "&amp;");
            //};

            return decodedUrl;
        }
    }
}
