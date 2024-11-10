using Yavsc.Abstract.Identity.Security;

namespace Yavsc.ViewModels.Relationship
{
    public class CirclesViewModel
    {
        public CirclesViewModel(ICircleAuthorized resource)
        {
            Target = resource;
            if (resource!=null)
                TargetTypeName = resource.GetType().Name;
        }
        public ICircleAuthorized Target { get; set; }
        public string TargetTypeName { get; set; }
    }
}
