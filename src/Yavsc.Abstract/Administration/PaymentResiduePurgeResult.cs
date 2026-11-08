namespace Yavsc.Abstract.Administration;

/// <summary>Reports technical subscription records purged for withdrawn accounts.</summary>
/// <summary>Counts accounts and technical payment records erased by the retention purge, excluding accounting documents.</summary>
public sealed record PaymentResiduePurgeResult(int Users, int Subscriptions, int Payments, int Donations = 0, int LegacyPayments = 0);
