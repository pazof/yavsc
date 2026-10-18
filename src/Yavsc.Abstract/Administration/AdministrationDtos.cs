#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Yavsc.Abstract.Administration;

/// <summary>Site administration counts and runtime information.</summary>
public sealed record AdministrationSummary(int UserCount, int AdministratorCount, string Assembly, string Runtime);

/// <summary>A role with its current membership count.</summary>
public sealed record AdministrationRole(string Id, string Name, int UserCount);

/// <summary>A user belonging to an administration role.</summary>
public sealed record RoleMember(string Id, string UserName, string? Email);

/// <summary>Access token format understood by IdentityServer.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ClientAccessTokenType>))]
public enum ClientAccessTokenType { Jwt, Reference }

/// <summary>Refresh token reuse policy.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ClientRefreshTokenUsage>))]
public enum ClientRefreshTokenUsage { ReUse, OneTimeOnly }

/// <summary>Refresh token expiration policy.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ClientRefreshTokenExpiration>))]
public enum ClientRefreshTokenExpiration { Sliding, Absolute }

/// <summary>Editable OAuth/OIDC scalar settings, independent of persistence entities.</summary>
public sealed record ClientSettings
{
    [Required, MaxLength(200)] public string ClientId { get; init; } = "";
    public bool Enabled { get; init; } = true;
    public string? ClientName { get; init; }
    public string? Description { get; init; }
    public string? ClientUri { get; init; }
    public string? LogoUri { get; init; }
    [Required] public string ProtocolType { get; init; } = "oidc";
    public bool RequireClientSecret { get; init; } = true;
    public bool RequireConsent { get; init; }
    public bool AllowRememberConsent { get; init; } = true;
    public bool AlwaysIncludeUserClaimsInIdToken { get; init; }
    public bool RequirePkce { get; init; } = true;
    public bool AllowPlainTextPkce { get; init; }
    public bool RequireRequestObject { get; init; }
    public bool AllowAccessTokensViaBrowser { get; init; }
    public string? FrontChannelLogoutUri { get; init; }
    public bool FrontChannelLogoutSessionRequired { get; init; } = true;
    public string? BackChannelLogoutUri { get; init; }
    public bool BackChannelLogoutSessionRequired { get; init; } = true;
    public bool AllowOfflineAccess { get; init; }
    [Range(1, int.MaxValue)] public int IdentityTokenLifetime { get; init; } = 300;
    [Range(1, int.MaxValue)] public int AccessTokenLifetime { get; init; } = 3600;
    [Range(1, int.MaxValue)] public int AuthorizationCodeLifetime { get; init; } = 300;
    [Range(1, int.MaxValue)] public int AbsoluteRefreshTokenLifetime { get; init; } = 2592000;
    [Range(1, int.MaxValue)] public int SlidingRefreshTokenLifetime { get; init; } = 1296000;
    [Range(1, int.MaxValue)] public int? ConsentLifetime { get; init; }
    [EnumDataType(typeof(ClientAccessTokenType))] public ClientAccessTokenType AccessTokenType { get; init; }
    [EnumDataType(typeof(ClientRefreshTokenExpiration))]
    public ClientRefreshTokenExpiration RefreshTokenExpiration { get; init; } = ClientRefreshTokenExpiration.Absolute;
    [EnumDataType(typeof(ClientRefreshTokenUsage))]
    public ClientRefreshTokenUsage RefreshTokenUsage { get; init; } = ClientRefreshTokenUsage.OneTimeOnly;
    public bool UpdateAccessTokenClaimsOnRefresh { get; init; }
    public bool EnableLocalLogin { get; init; } = true;
    public bool IncludeJwtId { get; init; } = true;
    public bool AlwaysSendClientClaims { get; init; }
    public string? ClientClaimsPrefix { get; init; } = "client_";
    public string? PairWiseSubjectSalt { get; init; }
    [Range(1, int.MaxValue)] public int? UserSsoLifetime { get; init; }
    public string? UserCodeType { get; init; }
    [Range(1, int.MaxValue)] public int DeviceCodeLifetime { get; init; } = 300;
    public string? AllowedIdentityTokenSigningAlgorithms { get; init; }
}

/// <summary>A client configuration collection supported by the web administration.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ClientCollection>))]
public enum ClientCollection
{
    RedirectUris, PostLogoutRedirectUris, Scopes, GrantTypes, CorsOrigins,
    IdentityProviders, Claims, Properties
}

/// <summary>An editable collection entry; Key is used for claims and properties.</summary>
public sealed record ClientEntry
{
    public string? Key { get; init; }
    [Required, MaxLength(4096)] public string Value { get; init; } = "";
}

/// <summary>A persisted collection row, without any credential material.</summary>
public sealed record ClientEntryResponse(int Id, ClientCollection Collection, string? Key, string Value);

/// <summary>Secret metadata; neither plaintext nor a stored hash is exposed.</summary>
public sealed record ClientSecretInfo(int Id, string? Description, DateTimeOffset Created, DateTimeOffset? Expiration);

/// <summary>A client and all its administrable collections.</summary>
public sealed record AdministrationClient(int Id, ClientSettings Settings,
    IReadOnlyList<ClientEntryResponse> Entries, IReadOnlyList<ClientSecretInfo> Secrets);

/// <summary>Input for adding a shared secret or generating a replacement.</summary>
public sealed record ClientSecretRequest
{
    [MaxLength(4096)] public string? Value { get; init; }
    [MaxLength(2000)] public string? Description { get; init; }
    public DateTimeOffset? Expiration { get; init; }
}

/// <summary>A newly generated client secret returned once, never available through GET.</summary>
public sealed record NewClientSecret(int Id, string Value, DateTimeOffset? Expiration);

/// <summary>Editable API scope settings.</summary>
public sealed record ApiScopeSettings
{
    [Required, MaxLength(200)] public string Name { get; init; } = "";
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public bool Enabled { get; init; } = true;
    public bool Required { get; init; }
    public bool Emphasize { get; init; }
    public bool ShowInDiscoveryDocument { get; init; } = true;
}

/// <summary>A persisted API scope.</summary>
public sealed record AdministrationScope(int Id, ApiScopeSettings Settings);

/// <summary>Mail delivery periodicity, compatible with the server calendar model.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MailPeriodicity>))]
public enum MailPeriodicity { OnDemand = -1, Daily, Weekly, Monthly, ThreeM, SixM, Yearly }

/// <summary>Editable mail template settings.</summary>
public sealed record MailingTemplateSettings
{
    [Required, StringLength(256, MinimumLength = 3)] public string Id { get; init; } = "";
    [Required, StringLength(256, MinimumLength = 3)] public string Topic { get; init; } = "";
    [Required, MaxLength(65536)] public string Body { get; init; } = "";
    [Required, EmailAddress] public string ReplyToAddress { get; init; } = "";
    [EnumDataType(typeof(MailPeriodicity))] public MailPeriodicity ToSend { get; init; } = MailPeriodicity.OnDemand;
}

/// <summary>A safe account business-data transfer without credentials, confirmations or roles.</summary>
public sealed record UserBusinessData
{
    [Required] public string Id { get; init; } = "";
    [Required, MaxLength(256)] public string UserName { get; init; } = "";
    [Required, EmailAddress] public string Email { get; init; } = "";
    [MaxLength(512)] public string? FullName { get; init; }
    [MaxLength(512)] public string? Avatar { get; init; }
    [MaxLength(512)] public string? DedicatedGoogleCalendar { get; init; }
    public bool AllowMonthlyEmail { get; init; }
    [Range(0, long.MaxValue)] public long DiskQuota { get; init; } = 536870912;
    [Range(0, long.MaxValue)] public long MaxFileSize { get; init; } = 536870912;
}

/// <summary>A blog business-data transfer retaining publication and access-control references.</summary>
public sealed record BlogBusinessData
{
    [Range(1, long.MaxValue)] public long Id { get; init; }
    [Required, MaxLength(1024)] public string Title { get; init; } = "";
    [MaxLength(56224)] public string? Article { get; init; }
    [MaxLength(1024)] public string? Photo { get; init; }
    [Required] public string AuthorId { get; init; } = "";
    [JsonRequired] public bool Published { get; init; }
    public DateTimeOffset DateCreated { get; init; }
    [JsonRequired] public long[] CircleIds { get; init; } = [];
    public string[] Tags { get; init; } = [];
}

/// <summary>An activity business-data transfer without navigation entities.</summary>
public sealed record ActivityBusinessData
{
    [Required, MaxLength(512)] public string Code { get; init; } = "";
    [Required, MaxLength(512)] public string Name { get; init; } = "";
    [MaxLength(512)] public string? ParentCode { get; init; }
    public string? Description { get; init; }
    public string? Photo { get; init; }
    public bool Moderated { get; init; }
    public string? ModeratorGroupName { get; init; }
    [Range(0, 100)] public int Rate { get; init; }
    public string? SettingsClassName { get; init; }
    public bool Hidden { get; init; }
}

/// <summary>A versioned business-data transfer. No authentication secrets are part of this schema.</summary>
public sealed record AdministrationTransfer
{
    [JsonRequired, Range(1, 1)] public int Version { get; init; } = 1;
    [JsonRequired, Required] public UserBusinessData[] Users { get; init; } = [];
    [JsonRequired, Required] public BlogBusinessData[] Blogs { get; init; } = [];
    [JsonRequired, Required] public ActivityBusinessData[] Activities { get; init; } = [];
}

/// <summary>Counts of rows successfully imported in a single transaction.</summary>
public sealed record ImportResult(int Users, int Blogs, int Activities);
