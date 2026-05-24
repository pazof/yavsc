// Yavsc.Controllers.Kyc/ModerationController.cs
namespace Yavsc.Controllers.Kyc
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Yavsc.Server.Helpers;
    using Yavsc.Services.Kyc;
    using Yavsc.ViewModels.Kyc;

    /// <summary>
    /// Accessible uniquement au rôle "Moderator".
    /// Voit les déclarations en attente + flags, prend les décisions.
    /// </summary>
    [Authorize(Roles = "Moderator")]
    [Route("kyc/moderation")]
    public class ModerationController : Controller
    {
        private readonly IModerationPipelineService _pipeline;

        public ModerationController(IModerationPipelineService pipeline)
        {
            _pipeline = pipeline;
        }

        /// <summary>
        /// GET kyc/moderation — queue de modération
        /// Flagged en tête, puis Pending par date de soumission.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var queue = await _pipeline.GetQueueAsync();
            return View(queue);
        }

        /// <summary>
        /// POST kyc/moderation/approve
        /// </summary>
        [HttpPost("approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ModerationDecisionModel input)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            var moderatorId = User.GetUserId();
            await _pipeline.ApproveAsync(input.DeclarationId, input.ScoreDelta, moderatorId);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST kyc/moderation/reject
        /// </summary>
        [HttpPost("reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(ModerationDecisionModel input)
        {
            var moderatorId = User.GetUserId();
            await _pipeline.RejectAsync(input.DeclarationId, moderatorId);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST kyc/moderation/redact
        /// </summary>
        [HttpPost("redact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Redact(ModerationDecisionModel input)
        {
            var moderatorId = User.GetUserId();
            await _pipeline.RedactAsync(input.DeclarationId, input.ScoreDelta, moderatorId);
            return RedirectToAction("Index");
        }
    }
}