using System;

namespace Yavsc.Interfaces
{
 public interface ILifeTime
    {
        DateTime Modified { get; set; }
        DateTime Posted { get; set; }
    }
}
