
using Yavsc.Abstract.Identity.Security;

namespace Yavsc.Abstract.BlogSpot;

public class PostAccessControlRulePayload : CircleAuthorization
{
      public long BlogPostId { get; set; }
}
