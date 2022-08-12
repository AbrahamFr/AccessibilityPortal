using ComplianceSheriff.Exceptions;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace ComplianceSheriff.Authentication
{
    public class JwtPayloadHandler
    {
        private readonly JwtSignInHandler _jwtSignInHandler;

        public JwtPayloadHandler(JwtSignInHandler jwtSignInHandler)
        {   
            _jwtSignInHandler = jwtSignInHandler;
        }

        public JwtPayload GetJwtPayload(HttpContext context)
        {
            JwtPayload jwtPayload = null;
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                var jwtToken = context.Request.Headers["Authorization"].ToString().Split(' ')[1];

                if (string.IsNullOrWhiteSpace(jwtToken))
                {
                    throw new InvalidTokenException("invalidToken");
                }

                var jwtSecurityToken = _jwtSignInHandler.ReadJwt(jwtToken);
                jwtPayload = jwtSecurityToken.Payload;
            }

            return jwtPayload;
        }
    }
}
