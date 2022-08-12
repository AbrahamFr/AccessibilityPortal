using ComplianceSheriff.Authentication;
using ComplianceSheriff.UsageAudit;
using ComplianceSheriff.Work;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ComplianceSheriff.Extensions
{
    public static class RestAPIUtils
    {
        public static bool DeleteFiles(string fullFilePath)
        {
            bool result = false;

            var filePath = Path.GetDirectoryName(fullFilePath);

            if (Directory.Exists(filePath))
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    result = true;
                }
            }

            return result;
        }

        public static JwtPayload GetJwtPayload(HttpContext context, JwtSignInHandler jwtSignInHandler)
        {
            var jwtPayloadHandler = new JwtPayloadHandler(jwtSignInHandler);
            var jwtPayLoad = jwtPayloadHandler.GetJwtPayload(context);

            return jwtPayLoad;
        }
    }
}
