using System;

namespace Yavsc.Abstract.Identity
{
    public class TokenInfo
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { set; get; }
        public DateTime Received { get; set; }
        public string TokenType { get; set; }
    }
}