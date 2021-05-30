using System;
using System.Collections.Generic;
using Yavsc.Abstract.Templates;
using Yavsc.Models;

namespace Yavsc.Templates
{
    public static class TemplateConstants 
    {
        public static readonly Dictionary<string, Func<ApplicationUser, bool>> Criterias =
            new Dictionary<string, Func<ApplicationUser, bool>>
            {
                { "allow-monthly", u => u.AllowMonthlyEmail },
                { "email-not-confirmed", u => !u.EmailConfirmed && u.DateCreated < DateTime.Now.AddDays(-7) }
            };
    }

    public abstract class UserOrientedTemplate: Template
    {
        public ApplicationUser User { get; set; }
    }
}
