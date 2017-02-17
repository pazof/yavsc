
using System;

namespace ZicMoove.Model.Auth.Account
{
    using Yavsc.Models;
    using YavscLib;

    public class GoogleCloudMobileDeclaration 
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