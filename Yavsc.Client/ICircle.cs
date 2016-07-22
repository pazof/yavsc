﻿using System.Collections.Generic;

namespace Yavsc.Models
{
    public interface ICircle
    {
        long Id { get; set; }
        IList<ICircleMember> Members { get; set; }
        string Name { get; set; }
        IApplicationUser Owner { get; set; }
        string OwnerId { get; set; }
    }
}