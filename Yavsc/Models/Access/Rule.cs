

namespace Yavsc.Models.Access
{
    public abstract class Rule<TResource,TRequirement> 
    {
        public Rule()
        {
            
        }
        // Abstract method to compute any authorization on a resource
        public abstract bool Allow(string userId, TResource resource, TRequirement requirement);
    }
}
