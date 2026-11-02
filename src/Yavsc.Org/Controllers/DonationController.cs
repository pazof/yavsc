#nullable enable

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Models.Payment;
using Yavsc.Models.Workflow;
using Yavsc.Server.Helpers;
using Yavsc.ViewModels.Podcast;
using Yavsc.Web.Services;

namespace Yavsc.Controllers;

[Authorize(Policy = "WebUser")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class DonationController(ApplicationDbContext db, IPayPalDonationGateway paypal,
    IOptions<SiteSettings> siteOptions, ILogger<DonationController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await db.PerformerDonations.AsNoTracking().Where(d => d.DonorId == User.GetUserId())
            .OrderByDescending(d => d.CreatedAt)
            .Join(db.Users, d => d.PerformerId, u => u.Id,
                (d, u) => new DonationDetailsViewModel(d, u.UserName ?? d.PerformerId)).ToArrayAsync(ct));

    [HttpGet]
    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        var performer = await db.Performers.FindAsync([User.GetUserId()], ct);
        if (performer == null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            ViewData["PaymentError"] = "Creez d'abord votre profil performer dans Mon compte > Parametres professionnels pour recevoir des dons.";
            return View("Error");
        }
        return View(new DonationSettingsViewModel { PayPalEmail = performer.DonationPayPalEmail });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(DonationSettingsViewModel model, CancellationToken ct)
    {
        var performer = await db.Performers.FindAsync([User.GetUserId()], ct);
        if (performer == null) return NotFound();
        if (!ModelState.IsValid) return View(model);
        performer.DonationPayPalEmail = string.IsNullOrWhiteSpace(model.PayPalEmail) ? null : model.PayPalEmail.Trim();
        await db.SaveChangesAsync(ct);
        TempData["DonationSettingsSaved"] = true;
        return RedirectToAction(nameof(Settings));
    }

    [HttpGet]
    public async Task<IActionResult> Create(string id, CancellationToken ct)
    {
        var performer = await RecipientAsync(id, ct);
        if (performer == null) return NotFound();
        return View(new CreateDonationViewModel { PerformerId = id, PerformerName = performer.Performer.UserName ?? id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDonationViewModel model, CancellationToken ct)
    {
        var performer = await RecipientAsync(model.PerformerId, ct);
        if (performer == null) return NotFound();
        model.PerformerName = performer.Performer.UserName ?? performer.PerformerId;
        if (decimal.Round(model.Amount, 2) != model.Amount)
            ModelState.AddModelError(nameof(model.Amount), "Le montant doit avoir au maximum deux decimales.");
        if (!ModelState.IsValid) return View(model);
        var urls = PaymentUrls();
        var donation = new PerformerDonation
        {
            Id = Guid.NewGuid(), DonorId = User.GetUserId(), PerformerId = performer.PerformerId,
            RecipientEmail = performer.DonationPayPalEmail!, Amount = model.Amount, CreatedAt = DateTimeOffset.UtcNow
        };
        db.PerformerDonations.Add(donation);
        await db.SaveChangesAsync(ct);
        donation.CheckoutToken = await paypal.CreateAsync(donation,
            urls.Return + donation.Id, urls.Cancel + donation.Id, urls.Notify, ct);
        donation.State = PerformerDonationState.AwaitingApproval;
        await SaveAsync(donation, ct);
        return Redirect(paypal.CheckoutUrl(donation.CheckoutToken));
    }

    [HttpGet]
    public async Task<IActionResult> Return(Guid id, string token, CancellationToken ct)
    {
        var donation = await OwnedAsync(id, ct);
        if (donation == null || string.IsNullOrWhiteSpace(token) || donation.CheckoutToken != token) return NotFound();
        if (donation.State == PerformerDonationState.AwaitingApproval)
            await paypal.ApproveAsync(donation, ct);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var donation = await OwnedAsync(id, ct);
        if (donation == null) return NotFound();
        var name = await db.Users.Where(u => u.Id == donation.PerformerId).Select(u => u.UserName).SingleAsync(ct);
        return View(new DonationDetailsViewModel(donation, name ?? donation.PerformerId));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var donation = await OwnedAsync(id, ct);
        if (donation == null) return NotFound();
        if (donation.State == PerformerDonationState.Completed)
            return RedirectToAction(nameof(Details), new { id });
        if (donation.State != PerformerDonationState.AwaitingApproval)
            throw new DonationPaymentException(409, "Ce paiement est deja traite ou en cours de verification. Ne creez pas un second don.");
        var approval = await paypal.ApproveAsync(donation, ct);
        donation.State = PerformerDonationState.Processing;
        await SaveAsync(donation, ct);
        var receipt = await paypal.CaptureAsync(donation, approval.PayerId, ct);
        // An IPN may have confirmed the payment while the capture request was in flight.
        await db.Entry(donation).ReloadAsync(ct);
        if (donation.TransactionId != null && donation.TransactionId != receipt.TransactionId)
            throw new DonationPaymentException(409, "La reference PayPal ne correspond pas au don.");
        if (donation.State is PerformerDonationState.Processing or PerformerDonationState.Pending)
        {
            donation.TransactionId = receipt.TransactionId;
            donation.State = receipt.Completed ? PerformerDonationState.Completed : PerformerDonationState.Pending;
            donation.CompletedAt = receipt.Completed ? DateTimeOffset.UtcNow : null;
            await SaveAsync(donation, ct);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var donation = await OwnedAsync(id, ct);
        if (donation == null) return NotFound();
        return View(donation);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirm(Guid id, CancellationToken ct)
    {
        var donation = await OwnedAsync(id, ct);
        if (donation == null) return NotFound();
        if (donation.State == PerformerDonationState.AwaitingApproval)
        {
            donation.State = PerformerDonationState.Cancelled;
            await SaveAsync(donation, ct);
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [AllowAnonymous, HttpPost, IgnoreAntiforgeryToken, RawPayPalNotification, RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> Notify(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync(ct);
        var fields = await paypal.VerifyNotificationAsync(body, ct);
        if (!Guid.TryParseExact(fields.GetValueOrDefault("invoice"), "N", out var id))
            throw new DonationPaymentException(400, "Reference de don PayPal invalide.");
        var donation = await db.PerformerDonations.FindAsync([id], ct);
        if (donation == null) return NotFound();
        var transactionId = fields.GetValueOrDefault("txn_id");
        if (fields.GetValueOrDefault("txn_type") != "express_checkout"
            || !string.Equals(fields.GetValueOrDefault("receiver_email"), donation.RecipientEmail, StringComparison.OrdinalIgnoreCase)
            || !PayPalDonationGateway.SameAmount(fields.GetValueOrDefault("mc_gross"), donation.Amount)
            || fields.GetValueOrDefault("mc_currency") != donation.Currency
            || string.IsNullOrWhiteSpace(transactionId) || transactionId.Length > 32
            || (donation.TransactionId != null && donation.TransactionId != transactionId)
            || donation.State is PerformerDonationState.Created or PerformerDonationState.AwaitingApproval or PerformerDonationState.Cancelled)
            throw new DonationPaymentException(409, "La notification PayPal ne correspond pas au don.");
        var paymentState = fields.GetValueOrDefault("payment_status");
        if (paymentState is not ("Completed" or "Pending" or "Denied" or "Failed" or "Expired" or "Voided"))
        {
            logger.LogWarning("PayPal donation {DonationId} notification has unsupported status {Status}", id, paymentState);
            return Ok();
        }

        if (donation.State != PerformerDonationState.Completed)
        {
            donation.TransactionId = transactionId;
            donation.State = paymentState switch
            {
                "Completed" => PerformerDonationState.Completed,
                "Pending" => PerformerDonationState.Pending,
                _ => PerformerDonationState.Failed
            };
            donation.CompletedAt = paymentState == "Completed" ? DateTimeOffset.UtcNow : null;
            await SaveAsync(donation, ct);
        }
        return Ok();
    }

    private Task<PerformerProfile?> RecipientAsync(string id, CancellationToken ct)
        => db.Performers.Include(p => p.Performer).SingleOrDefaultAsync(p => p.PerformerId == id
            && p.Active && p.DonationPayPalEmail != null && p.DonationPayPalEmail != "", ct);

    private Task<PerformerDonation?> OwnedAsync(Guid id, CancellationToken ct)
        => db.PerformerDonations.SingleOrDefaultAsync(d => d.Id == id && d.DonorId == User.GetUserId(), ct);

    private async Task SaveAsync(PerformerDonation donation, CancellationToken ct)
    {
        donation.Revision = Guid.NewGuid();
        await db.SaveChangesAsync(ct);
    }

    private (string Return, string Cancel, string Notify) PaymentUrls()
    {
        if (!Uri.TryCreate(siteOptions.Value.ExternalUrl, UriKind.Absolute, out var origin)
            || origin.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(origin.UserInfo))
            throw new DonationPaymentException(503, "L'adresse HTTPS du site n'est pas configuree.");
        var root = siteOptions.Value.ExternalUrl.TrimEnd('/') + "/";
        return (root + "Donation/Return?id=", root + "Donation/Cancel?id=", root + "Donation/Notify");
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        var error = executed.Exception;
        if (error == null || error is not (DonationPaymentException or DbUpdateConcurrencyException) || Response.HasStarted)
            return;
        logger.LogError(error, "Donation request failed for {Action}", context.ActionDescriptor.DisplayName);
        executed.ExceptionHandled = true;
        Response.StatusCode = error is DonationPaymentException payment ? payment.StatusCode : 409;
        ViewData["PaymentError"] = error is DonationPaymentException known
            ? known.Message : "Ce don a ete modifie par une autre requete. Consultez son etat avant de recommencer.";
        if (context.ActionArguments.TryGetValue("id", out var id) && id is Guid donationId)
            ViewData["DonationId"] = donationId;
        executed.Result = View("Error");
    }
}
