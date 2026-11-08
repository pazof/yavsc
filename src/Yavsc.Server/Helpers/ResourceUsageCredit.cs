namespace Yavsc.Server.Helpers;

public static class ResourceUsageCredit
{
    public static decimal NetConsumption(decimal gross, decimal credits) => Math.Max(0m, gross - credits);
    public static decimal Balance(decimal gross, decimal credits) => Math.Max(0m, credits - gross);

    public static decimal MonthlyGrant(decimal gross, decimal existingCredits, decimal allowance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(gross);
        ArgumentOutOfRangeException.ThrowIfNegative(existingCredits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(allowance);
        var erased = Math.Min(allowance, NetConsumption(gross, existingCredits));
        // Round down only the carried credit to the persistence precision.
        var carried = decimal.Round((allowance - erased) * 2m / 3m, 8, MidpointRounding.ToZero);
        return decimal.Round(erased + carried, 8, MidpointRounding.ToZero);
    }

    public static int BillingCycle(DateTimeOffset anchor, DateTimeOffset paidAt)
    {
        if (paidAt < anchor) throw new ArgumentOutOfRangeException(nameof(paidAt));
        var cycle = (paidAt.Year - anchor.Year) * 12 + paidAt.Month - anchor.Month;
        if (anchor.AddMonths(cycle) > paidAt) cycle--;
        return cycle;
    }
}
