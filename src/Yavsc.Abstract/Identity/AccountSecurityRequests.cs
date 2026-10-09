#nullable enable

namespace Yavsc.Abstract.Identity;

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class TwoFactorRequest
{
    public bool Enabled { get; set; }
}

public sealed class MonthlyEmailPreferenceRequest
{
    public bool Enabled { get; set; }
}

public sealed class DeleteAccountRequest
{
    public string UsernameConfirmation { get; set; } = string.Empty;
}

public sealed class LinkedExternalLogin
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public sealed class BankAccountInfo
{
    public long Id { get; set; }
    public string IBAN { get; set; } = string.Empty;
    public string BIC { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string WicketCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public int BankedKey { get; set; }
}

public sealed class BankAccountInfoRequest
{
    public string IBAN { get; set; } = string.Empty;
    public string BIC { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string WicketCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public int BankedKey { get; set; }
}

public sealed class CalendarOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
