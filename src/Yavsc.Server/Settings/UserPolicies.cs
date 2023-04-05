using System;
using System.Collections.Generic;
using Yavsc.Models;

namespace Yavsc.Server.Settings
{
    public class UserPolicies
    {
        public static readonly Dictionary<string, Func<ApplicationUser, bool>> Criterias =
            new Dictionary<string, Func<ApplicationUser, bool>>
            {
                { "allow-monthly", u => u.AllowMonthlyEmail },
                { "email-not-confirmed", u => !u.EmailConfirmed }
            };
    }
}
