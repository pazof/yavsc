#nullable enable

namespace Yavsc.Abstract.Identity {
  public class Me : UserInfo
  {
      public bool EmailConfirmed { get; set; }
      public bool AllowMonthlyEmail { get; set; }
      public bool HasPassword { get; set; }
      public bool TwoFactorEnabled { get; set; }
      public int ExternalLoginCount { get; set; }
      public long PostsCounter { get; set; }
      public long DiskUsage { get; set; }
      public long DiskQuota { get; set; }
      public decimal Credits { get; set; }
      public BankAccountInfo[] BankAccounts { get; set; } = [];
      public LinkedExternalLogin[] ExternalLogins { get; set; } = [];

      public AuthToken? Token {
          get;
          set;
      }
  }
}
