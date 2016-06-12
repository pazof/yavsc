using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Yavsc.Models.Auth
{
    public class AuthorisationView { 
        public Scope[] Scopes { get; set; }
        public string Message { get; set; }

    }
}