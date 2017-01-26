using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.Model.Auth.Account
{
    public class Tokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { set; get; }
        public DateTime Received { get; set; }
        public string TokenType { get; set; }
    }
}
