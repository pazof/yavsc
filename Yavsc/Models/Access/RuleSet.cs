using System;
using System.Collections.Generic;
using Microsoft.AspNet.Authorization;

namespace Yavsc.Models.Access
{

    public class RuleSet <TResource>:List<Rule<TResource>> where TResource : IAuthorizationRequirement{

        bool Allow(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

    }
}