// Yavsc.Controllers.Kyc/ConsultantController.cs
namespace Yavsc.Controllers.Kyc
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Yavsc.Models;
    using Yavsc.Models.Kyc;
    using Yavsc.Services.Kyc;
    using Yavsc.ViewModels.Kyc;

    /// <summary>
    /// Accessible aux utilisateurs avec le rôle "Consultant".
    /// Ne voit qu'un score agrégé — jamais de déclarations, jamais d'identité.
    /// </summary>
    [Authorize(Roles = "Consultant")]
    [Route("kyc/score")]
    public class ConsultantController : Controller
    {
        private readonly ITrustTokenService _tokenService;
        private readonly ApplicationDbContext _db;

        public ConsultantController(
            ITrustTokenService tokenService,
            ApplicationDbContext db)
        {
            _tokenService = tokenService;
            _db = db;
        }

        /// <summary>
        /// POST kyc/score
        /// Body : { subjectEmail } ou { externalToken }
        /// Retourne uniquement le score agrégé.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Query(ScoreQueryModel input)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            TrustToken token = string.IsNullOrEmpty(input.ExternalToken)
                ? await _tokenService.GetOrCreateFromEmailAsync(input.SubjectEmail)
                : await _tokenService.GetOrCreateFromTrustedPartyAsync(input.ExternalToken);

            // Chargement du score avec stats agrégées — aucune déclaration exposée
            var stats = await _db.TrustDeclarations
                .Where(d => d.TrustTokenId == token.Id
                         && d.Status == ModerationStatus.Approved)
                .GroupBy(d => d.TrustTokenId)
                .Select(g => new {
                    Count       = g.Count(),
                    LastUpdated = g.Max(d => d.SubmittedAt)
                })
                .FirstOrDefaultAsync();

            var view = new TrustScoreView
            {
                Score            = token.TrustScore ?? 50,
                Category         = ScoreToCategory(token.TrustScore ?? 50),
                DeclarationCount = stats?.Count ?? 0,
                LastUpdated      = stats?.LastUpdated ?? token.CreatedAt
            };

            return View(view);
        }

        private static string ScoreToCategory(int score) => score switch
        {
            >= 75 => "Fiable",
            >= 40 => "Prudence",
            _     => "Risque"
        };
    }
}