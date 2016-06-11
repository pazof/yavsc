using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Yavsc.Models.Auth
{
    public class AuthorisationView { 
        public Scope[] Scopes { get; set; }
        public string RedirectUrl { get; set; }
        public string Message { get; set; }
        public string ClientId {get; set; }
        public string State {get; set; }

        public string ResponseType { get; set; }

        public IDictionary<string,StringValues> QueryStringComponents { get; set; }
    }
}