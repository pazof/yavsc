//
//  GoogleHelpers.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Compute.v1;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;
using Yavsc.Models;
using Yavsc.Models.Calendar;
using Yavsc.Services;
using Yavsc.Server.Helpers;

namespace Yavsc.Helpers
{
    /// <summary>
    /// Google helpers.
    /// </summary>
    public static class GoogleHelpers
    {
        public static async Task<GoogleCredential> GetCredentialForApi(IEnumerable<string> scopes)
        {
            GoogleCredential credential = await GoogleCredential.GetApplicationDefaultAsync();
			var baseClientService = new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential
				};
			var compute = new ComputeService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential
				});
			if (credential.IsCreateScopedRequired)
			{
				credential = credential.CreateScoped(scopes);
			}
            return credential;
        }
        public static async Task<IdentityUserLogin<string>> GetGoogleUserLoginAsync(
            this ApplicationDbContext context, 
            string yavscUserId)
        {
            var user = context.Users.FirstOrDefaultAsync(u=>u.Id==yavscUserId);
            if (user==null) return null;
            var googleLogin = await context.UserLogins.FirstOrDefaultAsync(
                x => x.UserId == yavscUserId && x.LoginProvider == "Google"
            );
            return googleLogin;
        }
 public static async Task<UserCredential> GetGoogleCredential(GoogleAuthSettings googleAuthSettings, IDataStore store, string googleUserLoginKey)
       {
           if (string.IsNullOrEmpty(googleUserLoginKey))
               throw new InvalidOperationException("No Google login");
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer());
            var token = await store.GetAsync<TokenResponse>(googleUserLoginKey);
            // token != null
            var c = SystemClock.Default;
            if (token.IsExpired(c)) {
                token = await RefreshToken(googleAuthSettings, token);
            }
            return new UserCredential(flow, googleUserLoginKey, token);
        }
        public static async Task<Period[]> GetFreeTime (this ICalendarManager manager, string calId, DateTime startDate, DateTime endDate) 
        {
            var evlist = await manager.GetCalendarAsync(calId, startDate, endDate, null) ;
            var result = evlist.Items
            .Where(
                ev => ev.Transparency == "transparent"
                         )
            .Select( 
                ev => new Period {
                     Start = ev.Start.DateTime.Value,
                     End = ev.End.DateTime.Value
                  }
            );
            return result.ToArray();
        }

        public static async Task<TokenResponse> RefreshToken(this GoogleAuthSettings settings, TokenResponse oldResponse)
        {
            string ep = " https://www.googleapis.com/oauth2/v4/token";
            // refresh_token client_id client_secret grant_type=refresh_token
            try {
                using (var m = new SimpleJsonPostMethod(ep)) {
                    return await m.Invoke<TokenResponse>(
                        new { refresh_token= oldResponse.RefreshToken, client_id=Startup.GoogleWebClientConfiguration["web:client_id"],
                         client_secret=Startup.GoogleWebClientConfiguration["web:client_secret"],
                          grant_type="refresh_token" }
                    );
                }
            }
            catch (Exception ex) {
                throw new Exception ("No refresh token for Google service account",ex);
            }
        }
    }
}

