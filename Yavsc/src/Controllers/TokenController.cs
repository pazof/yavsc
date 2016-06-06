using System;
using System.Linq;
using Microsoft.AspNet.Mvc;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using System.Security.Principal;
using Microsoft.AspNet.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Yavsc.Auth;
using Microsoft.AspNet.Identity;
using Yavsc.Models;
using System.Threading.Tasks;

namespace Yavsc.Controllers
{
    [Produces("application/json"),AllowAnonymous]
    public class TokenController : Controller
    {
        private readonly TokenAuthOptions tokenOptions;
        private ILogger logger;
        UserManager<ApplicationUser> manager;
        SignInManager<ApplicationUser> signInManager;
        public class TokenResponse { 
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string grant_type { get; set; }

            public int entity_id { get; set; }
        }
        UserTokenProvider tokenProvider;
        
        public TokenController( UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
            IOptions<TokenAuthOptions> token_options, ILoggerFactory loggerFactory, UserTokenProvider tokenProvider)
        {
            this.manager = userManager;
            this.tokenOptions = token_options.Value;
            this.signInManager = signInManager;
            this.tokenProvider = tokenProvider;
            //this.bearerOptions = options.Value;
            //this.signingCredentials = signingCredentials;
            logger = loggerFactory.CreateLogger<TokenController>();
        }

        /// <summary>
        /// Check if currently authenticated. Will throw an exception of some sort which shoudl be caught by a general
        /// exception handler and returned to the user as a 401, if not authenticated. Will return a fresh token if
        /// the user is authenticated, which will reset the expiry.
        /// </summary>
        /// <returns></returns>
        [HttpGet,HttpPost,Authorize]
        [Route("~/api/token/get")]
        public async Task<dynamic> Get()
        {
            bool authenticated = false;
            string user = null;
            int entityId = -1;
            string token = null;
            DateTime? tokenExpires = default(DateTime?);
            var currentUser = User;
            if (currentUser != null)
            {
                authenticated = currentUser.Identity.IsAuthenticated;
                if (authenticated)
                {
                    user = User.GetUserId();
                    logger.LogInformation($"authenticated user:{user}");
                    
                    foreach (Claim c in currentUser.Claims) if (c.Type == "EntityID") entityId = Convert.ToInt32(c.Value);
                  
                    tokenExpires = DateTime.UtcNow.AddMinutes(2);
                    token = await GetToken("id_token", user, tokenExpires);
                    return new TokenResponse { access_token = token, expires_in = 3400, entity_id = entityId };
                }
            }
             return new { authenticated = false };
        }

        public class AuthRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        /// <summary>
        /// Request a new token for a given username/password pair.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost,Route("~/api/token/post")]
        public async Task<IActionResult> Post(AuthRequest req)
        {
            if (!ModelState.IsValid)
             return new BadRequestObjectResult(ModelState);
            // Obviously, at this point you need to validate the username and password against whatever system you wish.
            var signResult = await signInManager.PasswordSignInAsync(req.username, req.password,false,true);
            
            if (signResult.Succeeded)
            {
                DateTime? expires = DateTime.UtcNow.AddMinutes(tokenOptions.ExpiresIn);
                var token = await GetToken("id_token",User.GetUserId(), expires);
                return Ok(new TokenResponse {access_token = token, expires_in = 3400, grant_type="id_token" });
            }
            return new BadRequestObjectResult(new { authenticated = false } ) ;
        }

        private async Task<string> GetToken(string purpose, string userid, DateTime? expires)
        {
            // Here, you should create or look up an identity for the user which is being authenticated.
            // For now, just creating a simple generic identity.
            var identuser = await manager.FindByIdAsync(userid);
            
            return await tokenProvider.GenerateAsync(purpose,manager,identuser);
        }
    }
}
