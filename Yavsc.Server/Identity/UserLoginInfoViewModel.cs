using System;
using System.Collections.Generic;

namespace Yavsc.Model.Identity
{
    // Models returned by AccountController actions.

    public class UserLoginInfoViewModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}
