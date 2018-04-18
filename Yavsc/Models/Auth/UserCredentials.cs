
using Yavsc.Models.OAuth;

namespace Yavsc.Models.Auth {

    public class UserCredential {
        public string UserId { get; set; }

        public OAuth2Tokens Tokens { get; set; }
        public UserCredential(string userId, OAuth2Tokens tokens)
        {
            UserId = userId;
            Tokens = tokens;
        }

        public string GetHeader()
        {
            return Tokens.TokenType+" "+Tokens.AccessToken;
        }
    }

}