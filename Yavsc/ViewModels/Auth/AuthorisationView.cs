using System.Collections.Generic;

namespace Yavsc
{
    public class AuthorisationView { 
        public Application Application { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string RedirectUrl { get; set; }
        public string Message { get; set; }

    }
}