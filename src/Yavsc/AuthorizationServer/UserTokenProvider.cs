
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Identity;
using Yavsc.Models;
using Yavsc.Server;

namespace Yavsc.Auth  {

    public class UserTokenProvider : Microsoft.AspNet.Identity.IUserTokenProvider<ApplicationUser>
    {
        private MonoDataProtector protector=null;
        public MonoDataProtector Protector {
            get { return protector; } 
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            if ( user==null ) throw new InvalidOperationException("no user");
            var por = new MonoDataProtector(ServerConstants.ApplicationName, new string[] { purpose } );

            return Task.FromResult(por.Protect(UserStamp(user)));
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager, ApplicationUser user)
        {
            var por = new MonoDataProtector(ServerConstants.ApplicationName,new string[] { purpose } );
            var userStamp = por.Unprotect(token);
            Console.WriteLine ("Unprotected: "+userStamp);
            string [] values = userStamp.Split(';');
            return Task.FromResult ( user.Id == values[0] && user.Email == values[1] && user.UserName == values[2]);
        }

         public static string UserStamp(ApplicationUser user) {
            return $"{user.Id};{user.Email};{user.UserName}";
        }
    }
}
