
using System;
using Yavsc.Models;
using Yavsc.Models.Identity;

namespace BookAStar.Model.Auth.Account
{
    public class GoogleCloudMobileDeclaration : IGoogleCloudMobileDeclaration
    {
        public string GCMRegistrationId { get; set; }
        public string DeviceId { get; set; }
        public string Model { get; set; }
        public string Platform { get; set; }
        public string Version { get; set; }

        public IApplicationUser DeviceOwner
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string DeviceOwnerId
        {
            get;

            set;
        }
    }
}