using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Interfaces;

namespace Yavsc.Models.Messaging
{
    public interface IAnnounce : IOwned {
         Reason For { get; set; }
         string Message { get; set; }
    }
    
}