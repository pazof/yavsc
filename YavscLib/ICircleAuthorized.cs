

using System.Collections.Generic;

namespace YavscLib
{
    public interface ICircleAuthorized
    {
        long Id { get; set; }
        string GetOwnerId ();
        bool AuthorizeCircle(long circleId);
        ICircleAuthorization [] GetACL(); 

    }
}