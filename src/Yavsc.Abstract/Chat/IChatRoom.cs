using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Abstract.Chat
{
    public interface IChatRoom<TMod>  where TMod : IChatRoomAccess
    {
        [RegularExpression(@"^#?[a-zA-Z0-9'-']{3,10}$", ErrorMessage = "chan name cannot be validated.")]
        string Name { get; }

        [RegularExpression(@"^#?[a-zA-Z0-9'-']{3,10}$", ErrorMessage = "topic cannot be validated.")]
        string Topic { get ; set; }

        List<TMod> Administration { get; }
    }
}