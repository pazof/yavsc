using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Newtonsoft.Json.Linq;

namespace Yavsc.Helpers.Auth
{
    using Yavsc.Models;
    using Yavsc.Models.Auth;
    public static class ExternalAuthStoreHelper {

        public static Task<OAuth2Tokens> GetTokensAsync(this ApplicationDbContext context, string externalUserId)
        {
            if (string.IsNullOrEmpty(externalUserId))
            {
                throw new ArgumentException("externalUserId MUST have a value");
            }

            var item = context.OAuth2Tokens.FirstOrDefault(x => x.UserId == externalUserId);
            // TODO Refresh token

            return Task.FromResult(item);
        }

        public static Task StoreTokenAsync(this ApplicationDbContext context, string externalUserId, JObject response, string accessToken,
        string tokenType, string refreshToken, string expiresIn
        )
        {
            if (string.IsNullOrEmpty(externalUserId))
            {
                throw new ArgumentException("googleUserId MUST have a value");
            }

            var item = context.OAuth2Tokens.SingleOrDefaultAsync(x => x.UserId == externalUserId).Result;
            if (item == null)
            {
                context.OAuth2Tokens.Add(new OAuth2Tokens
                {
                    TokenType = "Bearer",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.Now.AddSeconds(int.Parse(expiresIn)),
                    UserId = externalUserId
                });
            }
            else
            {
                item.AccessToken = accessToken;
                item.Expiration = DateTime.Now.AddMinutes(int.Parse(expiresIn));
                if (refreshToken != null)
                    item.RefreshToken = refreshToken;
                context.OAuth2Tokens.Update(item);
            }
            context.SaveChanges(externalUserId);
            return Task.FromResult(0);
        }
  }
}
