using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yavsc.Models.Identity;

namespace ZicMoove.Model.Auth
{
    public class GCMRegIdDeclaration : IGCMDeclaration
    {
        public string DeviceId
        { get; set; }

        public string GCMRegistrationId
        { get; set; }

        public DateTime? LatestActivityUpdate
        { get; set; }

        public string Model
        { get; set; }

        public string Platform
        { get; set; }

        public string Version
        { get; set; }
    }
}
