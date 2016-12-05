using Microsoft.AspNet.Authorization;

namespace Yavsc.Models.Access
{
    public abstract class Rule<TResource> where TResource : IAuthorizationRequirement
    {
        public virtual bool Mandatory { get; set; }
        public abstract bool Allow(ApplicationUser user);
    }
}
