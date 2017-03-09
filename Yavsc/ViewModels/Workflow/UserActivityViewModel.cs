using Yavsc.Models.Workflow;

namespace Yavsc.ViewModels.Workflow
{
    public class UserActivityViewModel
    {
         public UserActivity Declaration { get; set; }        
         public bool NeedsSettings { get; set; }
         public bool HasSettings { get; set; }
    }
}