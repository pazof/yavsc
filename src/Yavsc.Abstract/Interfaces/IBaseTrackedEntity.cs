using System;

namespace Yavsc
{
    public interface IBaseTrackedEntity
    {
        DateTime DateCreated { get; set; }
        string UserCreated { get; set; }
        DateTime DateModified { get; set; }
        string UserModified { get; set; }
    }
}
