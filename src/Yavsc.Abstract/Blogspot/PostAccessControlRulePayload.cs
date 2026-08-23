
using Yavsc.Abstract.Identity.Security;

namespace Yavsc.Abstract.BlogSpot;

public class PostAccessControlRulePayload : ICircleAuthorization
{
      public long CircleId { get; set; }
      public long BlogPostId { get; set; }
}
