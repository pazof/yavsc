namespace Yavsc.Models.Billing;

/// <summary>
/// Who signed. <see cref="Pro"/> is the service provider's
/// signature on a devis or contract; <see cref="Client"/> is the
/// customer's signature. A single <see cref="Estimate"/> can
/// carry at most one signature per type at the latest version
/// (older versions are kept for audit and read as
/// "most-recent-wins" by the controller).
/// </summary>
public enum SignatureType
{
    Pro = 0,
    Client = 1,
}
