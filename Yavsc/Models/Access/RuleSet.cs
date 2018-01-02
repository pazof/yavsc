using System.Collections.Generic;

namespace Yavsc.Models.Access
{
    public abstract class RuleSet <TResource,TRequirement>:List<Rule<TResource,TRequirement>> {

        public abstract bool Allow(ApplicationDbContext context, string userId, TResource resource, TRequirement requirement);

    }
}