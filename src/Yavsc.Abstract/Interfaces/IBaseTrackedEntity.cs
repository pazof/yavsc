using System;

namespace Yavsc
{
    public interface ITrackedEntity
    {
        DateTime DateCreated { get; set; }
        string UserCreated { get; set; }
        DateTime DateModified { get; set; }
        string UserModified { get; set; }
    }
}
