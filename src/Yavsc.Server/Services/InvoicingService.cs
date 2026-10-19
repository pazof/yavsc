#nullable enable

using Microsoft.EntityFrameworkCore;
using Yavsc.Billing;
using Yavsc.Models;
using Yavsc.Models.Billing;

namespace Yavsc.Services;

public interface IInvoicingService
{
    Task<InvoiceResponse> GenerateAsync(long estimateId, string actorId, bool administrator, CancellationToken ct);
    Task<InvoiceResponse> GetAsync(long commandId, string actorId, bool administrator, CancellationToken ct);
    Task<InvoiceResponse> RecordOfflineAsync(long commandId, RecordInvoicePaymentRequest request, string actorId, bool administrator, CancellationToken ct);
    Task<InvoiceResponse> VerifyPayPalAsync(long commandId, string transactionId, string actorId, bool administrator, CancellationToken ct);
}

public sealed class InvoiceOperationException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class InvoicingService(ApplicationDbContext db, IPayPalTransactionVerifier payPal) : IInvoicingService
{
    public async Task<InvoiceResponse> GenerateAsync(long estimateId, string actorId, bool administrator, CancellationToken ct)
    {
        var estimate = await db.Estimates.Include(e => e.Bill).Include(e => e.Signatures)
            .SingleOrDefaultAsync(e => e.Id == estimateId, ct)
            ?? throw new InvoiceOperationException(404, "Estimate not found.");
        RequirePerformer(actorId, estimate.OwnerId, administrator);
        if (!estimate.CommandId.HasValue)
            throw new InvoiceOperationException(409, "The estimate must be attached to a command.");

        var command = await db.NominativeServiceCommands.SingleOrDefaultAsync(q => q.Id == estimate.CommandId, ct)
            ?? throw new InvoiceOperationException(409, "The estimate's command no longer exists.");
        if (command.InvoiceEstimateId == estimateId)
        {
            RequirePerformer(actorId, command.InvoicePerformerId!, administrator);
            return await ResponseAsync(command, ct);
        }
        if (command.InvoiceEstimateId.HasValue || command.AmountDue.HasValue || command.AmountPaid != 0m)
            throw new InvoiceOperationException(409, "This command already has an invoice or a balance requiring reconciliation.");
        if (command.ClientId != estimate.ClientId || command.PerformerId != estimate.OwnerId)
            throw new InvoiceOperationException(409, "The estimate and command must have the same client and performer.");
        if (estimate.ProviderValidationDate == default || estimate.ClientValidationDate == default
            || estimate.SignaturePro?.SignerId != estimate.OwnerId || estimate.SignatureClient?.SignerId != estimate.ClientId
            || estimate.SignaturePro.Strokes.Length == 0 || estimate.SignatureClient.Strokes.Length == 0)
            throw new InvoiceOperationException(409, "Both parties must have signed and validated the estimate.");
        if (estimate.Bill.Count == 0 || estimate.Bill.Any(line => line.Currency != "EUR" || line.Count <= 0
            || decimal.Round(line.UnitaryCost, 2) != line.UnitaryCost))
            throw new InvoiceOperationException(409, "The estimate requires EUR lines with positive quantities and prices in whole cents.");

        var lines = estimate.Bill.Select(line => new InvoiceLineResponse(
            line.Name, line.Description, line.Count, line.UnitaryCost)).ToList();
        decimal due;
        try
        {
            due = lines.Sum(line => checked(line.Count * line.UnitaryCost));
        }
        catch (OverflowException)
        {
            throw new InvoiceOperationException(409, "The estimate total is too large.");
        }
        if (due < 0m)
            throw new InvoiceOperationException(409, "The invoice total cannot be negative.");

        command.EstablishInvoice(estimateId, "EUR", lines);
        await SaveAsync(actorId, ct);
        return await ResponseAsync(command, ct);
    }

    public async Task<InvoiceResponse> GetAsync(long commandId, string actorId, bool administrator, CancellationToken ct)
    {
        var command = await LoadAsync(commandId, actorId, administrator, ct);
        return await ResponseAsync(command, ct);
    }

    public async Task<InvoiceResponse> RecordOfflineAsync(long commandId, RecordInvoicePaymentRequest request,
        string actorId, bool administrator, CancellationToken ct)
    {
        var command = await LoadAsync(commandId, actorId, administrator, ct);
        RequirePerformer(actorId, command.InvoicePerformerId!, administrator);
        if (string.IsNullOrWhiteSpace(request.Reference) || request.Reference.Length > 128
            || request.Reference != request.Reference.Trim() || request.Amount <= 0m
            || decimal.Round(request.Amount, 2) != request.Amount
            || request.Currency != command.InvoiceCurrency || request.ReceivedAtUtc == default
            || request.ReceivedAtUtc > DateTimeOffset.UtcNow)
            throw new InvoiceOperationException(400, "A positive receipt, invoice currency, reference and non-future receipt date are required.");
        return await RecordAsync(command, "Offline", command.InvoicePerformerId!, request.Reference, request.Amount,
            request.Currency, request.ReceivedAtUtc.ToUniversalTime(), actorId, ct);
    }

    public async Task<InvoiceResponse> VerifyPayPalAsync(long commandId, string transactionId,
        string actorId, bool administrator, CancellationToken ct)
    {
        var command = await LoadAsync(commandId, actorId, administrator, ct);
        if (string.IsNullOrWhiteSpace(transactionId) || transactionId.Length > 128 || transactionId != transactionId.Trim())
            throw new InvoiceOperationException(400, "A PayPal transaction identifier is required.");
        var receipt = await payPal.VerifyAsync(transactionId, command.GetInvoiceId(), command.InvoiceCurrency!, ct);
        return await RecordAsync(command, "PayPal", "", receipt.TransactionId, receipt.Amount,
            receipt.Currency, receipt.ReceivedAtUtc, actorId, ct);
    }

    private async Task<InvoiceResponse> RecordAsync(NominativeServiceCommand command, string source, string scope,
        string reference, decimal amount, string currency, DateTimeOffset receivedAtUtc, string actorId, CancellationToken ct)
    {
        // PostgreSQL persists microseconds; normalize before comparing a replay.
        receivedAtUtc = new DateTimeOffset(receivedAtUtc.UtcTicks - receivedAtUtc.UtcTicks % 10, TimeSpan.Zero);
        var existing = await db.InvoicePayments.SingleOrDefaultAsync(
            payment => payment.Source == source && payment.ReferenceScope == scope && payment.Reference == reference, ct);
        if (existing is not null)
        {
            if (existing.CommandId != command.Id || existing.Amount != amount || existing.Currency != currency
                || existing.ReceivedAtUtc != receivedAtUtc)
                throw new InvoiceOperationException(409, "The receipt reference is already used with different details.");
            return await ResponseAsync(command, ct);
        }

        var payments = await db.InvoicePayments.Where(payment => payment.CommandId == command.Id).ToListAsync(ct);
        var total = payments.Sum(payment => payment.Amount);
        if (command.AmountPaid != total)
            throw new InvoiceOperationException(409, "The recorded balance requires reconciliation with the receipt history.");
        try
        {
            command.SetPaymentAmounts(command.AmountDue!.Value, checked(total + amount));
        }
        catch (OverflowException)
        {
            throw new InvoiceOperationException(400, "The cumulative payment amount is too large.");
        }
        db.InvoicePayments.Add(new InvoicePayment
        {
            CommandId = command.Id, Source = source, ReferenceScope = scope, Reference = reference,
            Amount = amount, Currency = currency, ReceivedAtUtc = receivedAtUtc,
            RecordedAtUtc = DateTimeOffset.UtcNow, RecordedBy = actorId
        });
        await SaveAsync(actorId, ct);
        return await ResponseAsync(command, ct);
    }

    private async Task<NominativeServiceCommand> LoadAsync(long commandId, string actorId, bool administrator, CancellationToken ct)
    {
        var command = await db.NominativeServiceCommands.SingleOrDefaultAsync(q => q.Id == commandId, ct)
            ?? throw new InvoiceOperationException(404, "Invoice not found.");
        if (!command.InvoiceEstimateId.HasValue)
            throw new InvoiceOperationException(404, "Invoice not found.");
        if (!administrator && actorId != command.InvoiceClientId && actorId != command.InvoicePerformerId)
            throw new InvoiceOperationException(403, "Only the invoice parties may access it.");
        return command;
    }

    private async Task<InvoiceResponse> ResponseAsync(NominativeServiceCommand command, CancellationToken ct)
    {
        var payments = await db.InvoicePayments.AsNoTracking().Where(payment => payment.CommandId == command.Id)
            .OrderBy(payment => payment.Id).ToListAsync(ct);
        var summary = new PaymentSummary(command.AmountDue
            ?? throw new InvalidOperationException("An established invoice must have a balance."),
            payments.Sum(payment => payment.Amount));
        var lines = command.GetInvoiceBillItems().Select(line =>
            new InvoiceLineResponse(line.Name, line.Description, line.Count, line.UnitaryCost)).ToList();
        return new InvoiceResponse(command.Id, command.InvoiceEstimateId!.Value, command.GetInvoiceId(),
            command.InvoiceClientId!, command.InvoicePerformerId!, command.InvoicedAtUtc!.Value, command.InvoiceCurrency!,
            summary.AmountDue, summary.AmountPaid, summary.AmountRemaining, summary.Status.ToString(), summary.IsAcquitted,
            lines, payments.Select(payment => new InvoicePaymentResponse(
                payment.Id, payment.Source, payment.Reference, payment.Amount, payment.Currency,
                payment.ReceivedAtUtc, payment.RecordedAtUtc, payment.RecordedBy)).ToList());
    }

    private async Task SaveAsync(string actorId, CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(actorId, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvoiceOperationException(409, "The invoice changed concurrently. Retry the same request.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is
            System.Data.Common.DbException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation })
        {
            throw new InvoiceOperationException(409, "The invoice or receipt reference already exists. Retry the same request.");
        }
    }

    private static void RequirePerformer(string actorId, string performerId, bool administrator)
    {
        if (!administrator && actorId != performerId)
            throw new InvoiceOperationException(403, "Only the performer may perform this operation.");
    }
}
