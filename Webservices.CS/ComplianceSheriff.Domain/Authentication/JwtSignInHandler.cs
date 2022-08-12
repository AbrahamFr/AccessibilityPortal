using ComplianceSheriff.JWTToken;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace ComplianceSheriff.Authentication
{
    public class JwtSignInHandler
    {
        public const string TokenAudience = "DemoUser";
        public const string TokenIssuer = "AppGate";
        private readonly SymmetricSecurityKey key;

        public JwtSignInHandler(SymmetricSecurityKey symmetricKey)
        {
            this.key = symmetricKey;
        }

        public string BuildJwt(DateTimeOffset expiration, IJWTTokenIdentityManagerNetCore jWTTokenIdentityManager)
        {
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claimsIdentity = jWTTokenIdentityManager.GetIdentity();

            var token = new JwtSecurityToken(
                issuer: TokenIssuer,
                audience: TokenAudience,
                claims: claimsIdentity.Claims,
                expires: expiration.UtcDateTime,                
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken ReadJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
    }
}
