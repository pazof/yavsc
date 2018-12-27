using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Yavsc.Helpers.Google {
    using Yavsc.Models;
    using Yavsc.Models.Auth;
  public static class GoogleStoreHelper {

        public static Task<OAuth2Tokens> GetTokensAsync(this ApplicationDbContext context, string googleUserId)
        {
            if (string.IsNullOrEmpty(googleUserId))
            {
                throw new ArgumentException("email MUST have a value");
            }

            var item = context.Tokens.FirstOrDefault(x => x.UserId == googleUserId);
            // TODO Refresh token

            return Task.FromResult(item);
        }

        public static Task StoreTokenAsync(this ApplicationDbContext context, string googleUserId, JObject response, string accessToken,
        string tokenType, string refreshToken, string expiresIn
        )
        {
            if (string.IsNullOrEmpty(googleUserId))
            {
                throw new ArgumentException("googleUserId MUST have a value");
            }

            var item = context.Tokens.SingleOrDefaultAsync(x => x.UserId == googleUserId).Result;
            if (item == null)
            {
                context.Tokens.Add(new OAuth2Tokens
                {
                    TokenType = "Bearer",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.Now.AddSeconds(int.Parse(expiresIn)),
                    UserId = googleUserId
                });
            }
            else
            {
                item.AccessToken = accessToken;
                item.Expiration = DateTime.Now.AddMinutes(int.Parse(expiresIn));
                if (refreshToken != null)
                    item.RefreshToken = refreshToken;
                context.Tokens.Update(item);
            }
            context.SaveChanges(googleUserId);
            return Task.FromResult(0);
        }
  }
}
