//
//  AccountController.cs
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
using System.Web.Http;
using System.Net.Http;
using Yavsc.Model.RolesAndMembers;
using System.Web.Security;
using System.Web.Profile;
using Yavsc.Helpers;
using System.Collections.Specialized;
using Yavsc.App_Start;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using Yavsc.Models.Identity;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Owin.Security.OAuth;
using Yavsc.Providers;
using System.Security.Cryptography;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace Yavsc.ApiControllers
{
	[Authorize]
	[RoutePrefix("api/Account")]
	public class AccountController : ApiController
	{
		private const string LocalLoginProvider = "Local";
		private ApplicationUserManager _userManager;

		public AccountController()
		{
		}

		public AccountController(ApplicationUserManager userManager,
			ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
		{
			UserManager = userManager;
			AccessTokenFormat = accessTokenFormat;
		}

		public ApplicationUserManager UserManager
		{
			get
			{
				return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
			}
			private set
			{
				_userManager = value;
			}
		}

		public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

		// GET api/Account/UserInfo
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("UserInfo")]
		public UserInfoViewModel GetUserInfo()
		{
			ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

			return new UserInfoViewModel
			{
				Email = User.Identity.GetUserName(),
				HasRegistered = externalLogin == null,
				LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
			};
		}

		// POST api/Account/Logout
		[Route("Logout")]
		public IHttpActionResult Logout()
		{
			Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
			return Ok();
		}

		// GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
		[Route("ManageInfo")]
		public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
		{
			IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

			if (user == null)
			{
				return null;
			}

			List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();

			foreach (IdentityUserLogin linkedAccount in user.Logins)
			{
				logins.Add(new UserLoginInfoViewModel
					{
						LoginProvider = linkedAccount.LoginProvider,
						ProviderKey = linkedAccount.ProviderKey
					});
			}

			if (user.PasswordHash != null)
			{
				logins.Add(new UserLoginInfoViewModel
					{
						LoginProvider = LocalLoginProvider,
						ProviderKey = user.UserName,
					});
			}

			return new ManageInfoViewModel
			{
				LocalLoginProvider = LocalLoginProvider,
				Email = user.UserName,
				Logins = logins,
				ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
			};
		}

		// POST api/Account/ChangePassword
		[Route("ChangePassword")]
		public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
				model.NewPassword);

			if (!result.Succeeded)
			{
				return GetErrorResult(result);
			}

			return Ok();
		}

		// POST api/Account/SetPassword
		[Route("SetPassword")]
		public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

			if (!result.Succeeded)
			{
				return GetErrorResult(result);
			}

			return Ok();
		}

		// POST api/Account/AddExternalLogin
		[Route("AddExternalLogin")]
		public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

			AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);

			if (ticket == null || ticket.Identity == null || (ticket.Properties != null
				&& ticket.Properties.ExpiresUtc.HasValue
				&& ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
			{
				return BadRequest("External login failure.");
			}

			ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

			if (externalData == null)
			{
				return BadRequest("The external login is already associated with an account.");
			}

			IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
				new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

			if (!result.Succeeded)
			{
				return GetErrorResult(result);
			}

			return Ok();
		}

		// POST api/Account/RemoveLogin
		[Route("RemoveLogin")]
		public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			IdentityResult result;

			if (model.LoginProvider == LocalLoginProvider)
			{
				result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
			}
			else
			{
				result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
					new UserLoginInfo(model.LoginProvider, model.ProviderKey));
			}

			if (!result.Succeeded)
			{
				return GetErrorResult(result);
			}

			return Ok();
		}

		// GET api/Account/ExternalLogin
		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
		[AllowAnonymous]
		[Route("ExternalLogin", Name = "ExternalLogin")]
		public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
		{
			if (error != null)
			{
				return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
			}

			if (!User.Identity.IsAuthenticated)
			{
				return new ChallengeResult(provider, this);
			}

			ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

			if (externalLogin == null)
			{
				return InternalServerError();
			}

			if (externalLogin.LoginProvider != provider)
			{
				Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
				return new ChallengeResult(provider, this);
			}

			ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
				externalLogin.ProviderKey));

			bool hasRegistered = user != null;

			if (hasRegistered)
			{
				Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

				ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
					OAuthDefaults.AuthenticationType);
				ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
					CookieAuthenticationDefaults.AuthenticationType);

				AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
				Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
			}
			else
			{
				IEnumerable<Claim> claims = externalLogin.GetClaims();
				ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
				Authentication.SignIn(identity);
			}

			return Ok();
		}

		// GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
		[AllowAnonymous]
		[Route("ExternalLogins")]
		public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
		{
			IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
			List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

			string state;

			if (generateState)
			{
				const int strengthInBits = 256;
				state = RandomOAuthStateGenerator.Generate(strengthInBits);
			}
			else
			{
				state = null;
			}

			foreach (AuthenticationDescription description in descriptions)
			{
				ExternalLoginViewModel login = new ExternalLoginViewModel
				{
					Name = description.Caption,
					Url = Url.Route("ExternalLogin", new
						{
							provider = description.AuthenticationType,
							response_type = "token",
							client_id = Startup.PublicClientId,
							redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
							state = state
						}),
					State = state
				};
				logins.Add(login);
			}

			return logins;
		}

		// POST api/Account/Register
		[AllowAnonymous]
		[Route("Register")]
		public async Task<IHttpActionResult> Register(RegisterBindingModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

			IdentityResult result = await UserManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				return GetErrorResult(result);
			}

			return Ok();
		}

		// POST api/Account/RegisterExternal
		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
		[Route("RegisterExternal")]
		public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var info = await Authentication.GetExternalLoginInfoAsync();
			if (info == null)
			{
				return InternalServerError();
			}

			var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

			IdentityResult result = await UserManager.CreateAsync(user);
			if (!result.Succeeded)
			{
				return GetErrorResult(result);
			}

			result = await UserManager.AddLoginAsync(user.Id, info.Login);
			if (!result.Succeeded)
			{
				return GetErrorResult(result); 
			}
			return Ok();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UserManager.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Helpers

		private IAuthenticationManager Authentication
		{
			get { return Request.GetOwinContext().Authentication; }
		}

		private IHttpActionResult GetErrorResult(IdentityResult result)
		{
			if (result == null)
			{
				return InternalServerError();
			}

			if (!result.Succeeded)
			{
				if (result.Errors != null)
				{
					foreach (string error in result.Errors)
					{
						ModelState.AddModelError("", error);
					}
				}

				if (ModelState.IsValid)
				{
					// No ModelState errors are available to send, so just return an empty BadRequest.
					return BadRequest();
				}

				return BadRequest(ModelState);
			}

			return null;
		}

		private class ExternalLoginData
		{
			public string LoginProvider { get; set; }
			public string ProviderKey { get; set; }
			public string UserName { get; set; }

			public IList<Claim> GetClaims()
			{
				IList<Claim> claims = new List<Claim>();
				claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

				if (UserName != null)
				{
					claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
				}

				return claims;
			}

			public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
			{
				if (identity == null)
				{
					return null;
				}

				Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

				if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
					|| String.IsNullOrEmpty(providerKeyClaim.Value))
				{
					return null;
				}

				if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
				{
					return null;
				}

				return new ExternalLoginData
				{
					LoginProvider = providerKeyClaim.Issuer,
					ProviderKey = providerKeyClaim.Value,
					UserName = identity.FindFirstValue(ClaimTypes.Name)
				};
			}
		}

		private static class RandomOAuthStateGenerator
		{
			private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

			public static string Generate(int strengthInBits)
			{
				const int bitsPerByte = 8;

				if (strengthInBits % bitsPerByte != 0)
				{
					throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
				}

				int strengthInBytes = strengthInBits / bitsPerByte;

				byte[] data = new byte[strengthInBytes];
				_random.GetBytes(data);
				return HttpServerUtility.UrlTokenEncode(data);
			}
		}

		#endregion

		/// <summary>
		/// Register the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[Authorize ()]
		[ValidateAjaxAttribute]
		public void Register ([FromBody] RegisterClientModel model)
		{
			
			if (ModelState.IsValid) {
				if (model.IsApprouved)
				if (!Roles.IsUserInRole ("Admin"))
				if (!Roles.IsUserInRole ("FrontOffice")) {
					ModelState.AddModelError ("Register", 
						"Since you're not member of Admin or FrontOffice groups, " +
						"you cannot ask for a pre-approuved registration");
					return ;
				}
				MembershipCreateStatus mcs;
				var user = Membership.CreateUser (
					           model.UserName,
					           model.Password,
					           model.Email,
					           model.Question,
					           model.Answer,
					           model.IsApprouved,
					           out mcs);
				switch (mcs) {
				case MembershipCreateStatus.DuplicateEmail:
					ModelState.AddModelError ("Email", "Cette adresse e-mail correspond " +
					"à un compte utilisateur existant");
					break;
				case MembershipCreateStatus.DuplicateUserName:
					ModelState.AddModelError ("UserName", "Ce nom d'utilisateur est " +
					"déjà enregistré");
					break;
				case MembershipCreateStatus.Success:
					if (!model.IsApprouved)
						Url.SendActivationMessage (user);
					ProfileBase prtu = ProfileBase.Create (model.UserName);
					prtu.SetPropertyValue ("Name", model.Name);
					prtu.SetPropertyValue ("Address", model.Address);
					prtu.SetPropertyValue ("CityAndState", model.CityAndState);
					prtu.SetPropertyValue ("Mobile", model.Mobile);
					prtu.SetPropertyValue ("Phone", model.Phone);
					prtu.SetPropertyValue ("ZipCode", model.ZipCode);
					break;
				default:
					break;
				}
			}
		}

		/// <summary>
		/// Resets the password.
		/// </summary>
		/// <param name="model">Model.</param>
		[ValidateAjax]
		public void ResetPassword (LostPasswordModel model)
		{
			if (ModelState.IsValid) 
			{
				StringDictionary errors;
				MembershipUser user;
				YavscHelpers.ValidatePasswordReset (model, out errors, out user);
				foreach (string key in errors.Keys)
					ModelState.AddModelError (key, errors [key]);
				if (user != null && ModelState.IsValid)
					Url.SendActivationMessage (user);
			}
		}

		/// <summary>
		/// Login the specified model.
		/// </summary>
		/// <param name="model">Model.</param>
		[ValidateAjax]
		public void Login(LoginModel model)
		{
			if (ModelState.IsValid) {
				if (Membership.ValidateUser (model.UserName, model.Password)) {
					FormsAuthentication.SetAuthCookie (model.UserName, model.RememberMe);
				}
			}
		}

		/// <summary>
		/// Adds the user to role.
		/// </summary>
		/// <param name="model">Model.</param>
		[ValidateAjax]
		[Authorize(Roles="Admin")]
		public void AddUserToRole(UserRole model)
		{
			if (ModelState.IsValid)
				Roles.AddUserToRole (model.UserName, model.Role);
		}

		/// <summary>
		/// Removes the user from role.
		/// </summary>
		/// <param name="model">Model.</param>
		[ValidateAjax]
		[Authorize(Roles="Admin")]
		public void RemoveUserFromRole(UserRole model)
		{
			if (ModelState.IsValid)
				Roles.RemoveUserFromRoles (model.UserName, 
					new string [] { model.Role } );
		}

		[ValidateAjax]
		[Authorize()]
		public void AddUserToCircle(UserRole model)
		{
			if (ModelState.IsValid)
				Roles.AddUserToRole (model.UserName, 
					 model.Role );
		}
	}
}
