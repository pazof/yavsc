using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yavsc.Billing;
using Yavsc.Server.Helpers;
using Yavsc.Services;

namespace Yavsc.ApiControllers;

[Authorize, ApiController, Route(Constants.APIPrefix + "/invoices")]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "invoicing")]
public sealed class InvoicesController(IInvoicingService invoices, ILogger<InvoicesController> logger) : ControllerBase
{
    /// <summary>Creates a frozen invoice from an estimate signed by both parties, or returns its existing invoice.</summary>
    [HttpPost("from-estimate/{estimateId:long}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ActionResult<InvoiceResponse>> Generate(long estimateId, CancellationToken ct)
        => ExecuteAsync(() => invoices.GenerateAsync(estimateId, User.GetUserId(), IsAdmin, ct), created: true);

    /// <summary>Returns the invoice, balance and receipt history to its parties.</summary>
    [HttpGet("{commandId:long}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ActionResult<InvoiceResponse>> Get(long commandId, CancellationToken ct)
        => ExecuteAsync(() => invoices.GetAsync(commandId, User.GetUserId(), IsAdmin, ct));

    /// <summary>Records an offline receipt attested by the performer, idempotently by reference.</summary>
    [HttpPost("{commandId:long}/payments/offline")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ActionResult<InvoiceResponse>> RecordOffline(long commandId, RecordInvoicePaymentRequest request, CancellationToken ct)
        => ExecuteAsync(() => invoices.RecordOfflineAsync(commandId, request, User.GetUserId(), IsAdmin, ct));

    /// <summary>Verifies a completed PayPal transaction server-side before recording its receipt.</summary>
    [HttpPost("{commandId:long}/payments/paypal")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public Task<ActionResult<InvoiceResponse>> VerifyPayPal(long commandId, VerifyInvoicePayPalRequest request, CancellationToken ct)
        => ExecuteAsync(() => invoices.VerifyPayPalAsync(commandId, request.TransactionId, User.GetUserId(), IsAdmin, ct));

    private bool IsAdmin => User.IsInRole(Constants.AdminName);

    private async Task<ActionResult<InvoiceResponse>> ExecuteAsync(Func<Task<InvoiceResponse>> action, bool created = false)
    {
        try
        {
            var invoice = await action();
            return created
                ? CreatedAtAction(nameof(Get), new { commandId = invoice.CommandId }, invoice)
                : Ok(invoice);
        }
        catch (InvoiceOperationException exception)
        {
            logger.LogWarning("Invoice operation rejected ({StatusCode}): {Reason}", exception.StatusCode, exception.Message);
            return Problem(statusCode: exception.StatusCode, title: exception.Message);
        }
    }
}
