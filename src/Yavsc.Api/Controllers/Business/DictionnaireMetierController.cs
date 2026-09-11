using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using Yavsc.Server.Helpers;
using Yavsc.Server.Services;

namespace Yavsc.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route(Constants.APIPrefix + "/dictionnaire-metier")]
    public class DictionnaireMetierController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DictionnaireMetierModerationService _moderationService;

        public DictionnaireMetierController(ApplicationDbContext context)
        {
            _context = context;
            _moderationService = new DictionnaireMetierModerationService(context);
        }

        [HttpGet("{activityCode}")]
        public async Task<ActionResult<IEnumerable<TermeMetier>>> GetTerms(
            [FromRoute] string activityCode,
            [FromQuery] string langue = "fr",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityCode))
            {
                return BadRequest("Activity code is required.");
            }

            var activityCodes = await ResolveActivityCodesAsync(activityCode, cancellationToken);
            if (activityCodes.Count == 0)
            {
                return NotFound();
            }

            var dictionaryIds = await _context.DictionnaireMetier
                .AsNoTracking()
                .Where(d => activityCodes.Contains(d.DomaineActiviteCode) && d.Langue == langue)
                .Select(d => d.Id)
                .ToListAsync(cancellationToken);

            if (dictionaryIds.Count == 0)
            {
                return Ok(new List<TermeMetier>());
            }

            var terms = await _context.TermeMetier
                .AsNoTracking()
                .Where(t => dictionaryIds.Contains(t.DictionnaireMetierId)
                    && t.StatutValidation == StatutValidationTerme.Valide)
                .OrderBy(t => t.Mot)
                .ToListAsync(cancellationToken);

            return Ok(terms);
        }

        [HttpGet("dictionnaires/{activityCode}")]
        public async Task<ActionResult<IEnumerable<DictionnaireMetier>>> GetDictionaries(
            [FromRoute] string activityCode,
            [FromQuery] string langue = "fr",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(activityCode))
            {
                return BadRequest("Activity code is required.");
            }

            var activityCodes = await ResolveActivityCodesAsync(activityCode, cancellationToken);
            if (activityCodes.Count == 0)
            {
                return NotFound();
            }

            var dictionaries = await _context.DictionnaireMetier
                .AsNoTracking()
                .Where(d => activityCodes.Contains(d.DomaineActiviteCode) && d.Langue == langue)
                .OrderBy(d => d.Nom)
                .ToListAsync(cancellationToken);

            return Ok(dictionaries);
        }

        [HttpPost("proposer")]
        public async Task<ActionResult<TermeMetier>> ProposeTerm(
            [FromBody] TermeMetier term,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(term.Mot) || string.IsNullOrWhiteSpace(term.Definition))
            {
                return BadRequest("Le terme et sa définition sont requis.");
            }

            var dictionary = await _context.DictionnaireMetier
                .SingleOrDefaultAsync(d => d.Id == term.DictionnaireMetierId, cancellationToken);

            if (dictionary is null)
            {
                return NotFound("Dictionary not found.");
            }

            var proposerId = User.GetUserId();
            var result = await _moderationService.ProposerTermAsync(
                term.DictionnaireMetierId,
                term.Mot,
                term.Definition,
                term.Langue,
                proposerId);

            return CreatedAtAction(nameof(GetTerms), new { activityCode = dictionary.DomaineActiviteCode }, result);
        }

        [HttpPut("{id}/valider")]
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> ValidateTerm(
            [FromRoute] long id,
            CancellationToken cancellationToken)
        {
            try
            {
                var term = await _moderationService.ValiderTermAsync(id, User.GetUserId());
                return Ok(term);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}/rejeter")]
        [Authorize("AdministratorOnly")]
        public async Task<IActionResult> RejectTerm(
            [FromRoute] long id,
            CancellationToken cancellationToken)
        {
            try
            {
                var term = await _moderationService.RejeterTermAsync(id, User.GetUserId());
                return Ok(term);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private async Task<List<string>> ResolveActivityCodesAsync(string activityCode, CancellationToken cancellationToken)
        {
            var result = new HashSet<string>();
            var currentCode = activityCode;

            while (!string.IsNullOrWhiteSpace(currentCode))
            {
                result.Add(currentCode);

                var current = await _context.Activities
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.Code == currentCode, cancellationToken);

                if (current is null || string.IsNullOrWhiteSpace(current.ParentCode))
                {
                    break;
                }

                currentCode = current.ParentCode;
            }

            return result.ToList();
        }
    }
}
