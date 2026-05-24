// Yavsc.Controllers.Kyc/DeclarantController.cs
namespace Yavsc.Controllers.Kyc
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Yavsc.Models;
    using Yavsc.Models.Kyc;
    using Yavsc.Services.Kyc;
    using Yavsc.ViewModels.Kyc;

    /// <summary>
    /// Accessible à tout utilisateur authentifié.
    /// Le déclarant soumet une déclaration sur un sujet pseudonymisé.
    /// </summary>
    [Authorize]
    [Route("kyc/declare")]
    public class DeclarantController : Controller
    {
        private readonly IModerationPipelineService _pipeline;
        private readonly ITrustTokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeclarantController(
            IModerationPipelineService pipeline,
            ITrustTokenService tokenService,
            UserManager<ApplicationUser> userManager)
        {
            _pipeline     = pipeline;
            _tokenService = tokenService;
            _userManager  = userManager;
        }

        /// <summary>
        /// GET kyc/declare — formulaire de déclaration
        /// </summary>
        [HttpGet]
        public IActionResult Index() => View();

        /// <summary>
        /// POST kyc/declare
        /// Body : { subjectEmail, content, sentiment }
        /// Le subjectEmail est immédiatement transformé en token — jamais stocké.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(DeclarationInputModel input)
        {
            if (!ModelState.IsValid)
                return View("Index", input);

            // Token du sujet — depuis email ou token tiers
            TrustToken subjectToken = string.IsNullOrEmpty(input.ExternalToken)
                ? await _tokenService.GetOrCreateFromEmailAsync(input.SubjectEmail)
                : await _tokenService.GetOrCreateFromTrustedPartyAsync(input.ExternalToken);

            // Token du déclarant — depuis son propre email confirmé
            var user = await _userManager.GetUserAsync(User);
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Forbid(); // Email non confirmé → pas de déclaration

            var declarantToken = await _tokenService
                .GetOrCreateFromEmailAsync(user.Email);

            await _pipeline.SubmitDeclarationAsync(
                subjectToken.Id,
                declarantToken.Id,
                input.Content,
                input.Sentiment);

            return RedirectToAction("Submitted");
        }

        [HttpGet("submitted")]
        public IActionResult Submitted() => View();
    }
}