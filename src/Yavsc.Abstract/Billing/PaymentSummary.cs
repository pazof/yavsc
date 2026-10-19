namespace Yavsc.Billing;

public sealed class PaymentSummary
{
    public PaymentSummary(decimal amountDue, decimal amountPaid)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amountDue);
        ArgumentOutOfRangeException.ThrowIfNegative(amountPaid);

        AmountDue = amountDue;
        AmountPaid = amountPaid;
    }

    public decimal AmountDue { get; }
    public decimal AmountPaid { get; }
    public decimal AmountRemaining => Math.Max(0m, AmountDue - AmountPaid);
    public bool IsAcquitted => AmountPaid >= AmountDue;
    public PaymentStatus Status => IsAcquitted
        ? PaymentStatus.Paid
        : AmountPaid > 0m ? PaymentStatus.PartiallyPaid : PaymentStatus.Unpaid;
}
