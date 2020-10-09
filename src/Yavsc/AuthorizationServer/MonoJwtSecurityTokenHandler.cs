


using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Yavsc.Auth
{

    public class MonoJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {

        public MonoJwtSecurityTokenHandler()
        {
        }
        public override JwtSecurityToken CreateToken(
   string issuer,
   string audience, ClaimsIdentity subject,
   DateTime? notBefore, DateTime? expires, DateTime? issuedAt,
   SigningCredentials signingCredentials
)
        {
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = audience,
                Claims = subject.Claims,
                Expires = expires,
                IssuedAt = issuedAt,
                Issuer = issuer,
                NotBefore = notBefore,
                SigningCredentials = signingCredentials
            };
            var token = base.CreateToken(tokenDescriptor);
            return token as JwtSecurityToken;
        }
    }

}
