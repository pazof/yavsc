using System;
using System.Collections.Generic;
using Yavsc.Abstract.Templates;
using Yavsc.Models;

namespace Yavsc.Templates
{
    public abstract class UserOrientedTemplate: Template
    {
        public ApplicationUser User { get; set; }
    }
}
