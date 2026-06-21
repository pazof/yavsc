# Client editor overhaul — Yavsc.Org administration

## Goal

Bring the OAuth2 client administration UI (`/Client/Edit/{id}` and friends)
in Yavsc.Org to feature parity with the IdentityServer8 `Client` entity
model. Today the editor only exposes a handful of scalar fields and a few
single-line inputs for collections; the bulk of the entity and its
related collections are unreachable from the UI.

## Inventory — current state

### Properties exposed by `Views/Client/Edit.cshtml`

| Field                    | Type        | Notes                              |
| ------------------------ | ----------- | ---------------------------------- |
| `ClientId`               | string      | hidden, identifier                 |
| `Enabled`                | bool        | checkbox                           |
| `ClientName`             | string      | display name                       |
| `FrontChannelLogoutUri`  | string      | only front-channel, no back-channel |
| `RedirectUris`           | collection  | rendered as a single text input    |
| `IdentityTokenLifetime`  | int         | seconds                            |
| `AbsoluteRefreshTokenLifetime` | int   | seconds                            |
| `ClientSecrets`          | collection  | rendered as a single text input    |
| `AccessTokenType`        | enum        | dropdown (custom `SetAppTypesInputValues`) |

### Properties of `IdentityServer8.EntityFramework.Entities.Client` **NOT** in the editor

Core scalars (16 fields missing):

- `Description`
- `ClientUri`
- `LogoUri`
- `RequireConsent`
- `RequirePkce`
- `RequireRequestObject`
- `RequireClientSecret`
- `AllowPlainTextPkce`
- `AllowOfflineAccess`
- `AllowRememberConsent`
- `AlwaysIncludeUserClaimsInIdToken`
- `AlwaysSendClientClaims`
- `AuthorizationCodeLifetime`
- `BackChannelLogoutUri`
- `BackChannelLogoutSessionRequired`
- `CibaLifetime`
- `ClientClaimsPrefix`
- `ConsentLifetime`
- `Created`
- `DeviceCodeLifetime`
- `EnableLocalLogin`
- `Enabled`
- `FrontChannelLogoutSessionRequired`
- `IncludeJwtId`
- `LastAccessed`
- `LogoUri`
- `NonEditable`
- `PairwiseSubjectSalt`
- `PollingInterval`
- `ProtocolType`
- `RefreshTokenExpiration`
- `RefreshTokenUsage`
- `SlidingRefreshTokenLifetime`
- `UpdateAccessTokenClaimsOnRefresh`
- `Updated`
- `UserCodeType`
- `UserSsoLifetime`

Collections (8 missing — currently either not exposed at all, or jammed
into a single-line text input that doesn't work for an IEnumerable):

- `AllowedGrantTypes`        → `ClientGrantType` (GrantType)
- `AllowedScopes`            → `ClientScope` (Scope)
- `RedirectUris`             → `ClientRedirectUri` (RedirectUri) — exposed but broken
- `PostLogoutRedirectUris`   → `ClientPostLogoutRedirectUri` (PostLogoutRedirectUri)
- `AllowedCorsOrigins`       → `ClientCorsOrigin` (Origin)
- `IdentityProviderRestrictions` → `ClientIdPRestriction` (Provider)
- `Claims`                   → `ClientClaim` (Type, Value)
- `Properties`               → `ClientProperty` (Key, Value)
- `ClientSecrets`            → `ClientSecret` (Type, Value, Description, Created, Expiration) — exposed but broken
- `AllowedSigningAlgorithms` → scalar string collection on Client itself

## Pages to add

Pattern: one Razor page per collection under
`Views/Client/Edit{Collection}.cshtml`. Each page lists existing rows,
offers an "Add" form with the relevant fields, and a per-row
remove button. The main `Edit.cshtml` becomes a hub page with links
to each subpage plus the scalar fields it already has.

| Page                                  | Route                                      | Form fields                                       |
| ------------------------------------- | ------------------------------------------ | ------------------------------------------------- |
| `Edit.cshtml`                         | `GET /Client/Edit/{id}` (existing)         | scalar fields + nav links                         |
| `EditRedirectUris.cshtml`             | `GET /Client/EditRedirectUris/{id}`        | `RedirectUri`                                     |
| `EditPostLogoutRedirectUris.cshtml`   | `GET /Client/EditPostLogoutRedirectUris/{id}` | `PostLogoutRedirectUri`                         |
| `EditScopes.cshtml`                   | `GET /Client/EditScopes/{id}`              | `Scope` (with select of known scopes)             |
| `EditGrantTypes.cshtml`               | `GET /Client/EditGrantTypes/{id}`          | `GrantType` (with select of known types)          |
| `EditCorsOrigins.cshtml`              | `GET /Client/EditCorsOrigins/{id}`         | `Origin`                                          |
| `EditIdPRestrictions.cshtml`          | `GET /Client/EditIdPRestrictions/{id}`     | `Provider`                                        |
| `EditClaims.cshtml`                   | `GET /Client/EditClaims/{id}`              | `Type`, `Value`                                   |
| `EditProperties.cshtml`               | `GET /Client/EditProperties/{id}`          | `Key`, `Value`                                    |
| `EditSecrets.cshtml` (replacement)    | `GET /Client/EditSecrets/{id}`             | `Type`, `Value`, `Description`, `Expiration`      |

Partial view `_EditableList.cshtml` factored once and consumed by all
of the above.

## Controller actions to add

For each collection `Foo`:

- `GET EditFoo(int id)` — load the client, render the page
- `POST AddFoo(int id, …)` — append a row, redirect to `EditFoo`
- `POST RemoveFoo(int id, int rowId)` — delete a row, redirect

## Verification

- `dotnet build src/Yavsc.Org/Yavsc.Org.csproj` → 0 errors
- No tests in `Yavsc.Org.Tests` exercise the controller today (per
  `find … -name "ClientController*" -not -path "*/bin/*"`). Smoke-test
  by logging in as admin, hitting `/Client/Edit/1`, then each
  `Edit*/1` page, and verifying the add/remove POSTs.
- Existing seed flow (`MigratePostItClientToPublic` in
  `HostingExtensions.cs`) must keep working — the editor changes are
  additive, not destructive.

## Out of scope

- Tests (no MVC test infrastructure currently exists for this controller)
- Migration of existing collection fields (the broken `RedirectUris`
  text input will simply be replaced by the new subpage)
- Per-collection authorization policies (the controller is already
  `[Authorize("AdministratorOnly")]`)
- Client cloning / templating / JSON import-export

## Status

2026-06-21 16:04 — kickoff. Inventory done. Pages not yet started.

2026-06-21 16:11 — first delivery, **build does not compile by design**
(per Paul: "Tu peux même me laisser un travail qui ne compile
pas"). The structural work is done; the residual errors are easy
fixes Paul will do in a debug session.

Files added (working tree, not yet committed):

- `src/Yavsc.Org/Controllers/Administration/ClientController.Collections.cs`
  — partial class with the per-collection GET / Add / Remove actions.
- `src/Yavsc.Org/Views/Client/EditRedirectUris.cshtml`
- `src/Yavsc.Org/Views/Client/EditPostLogoutRedirectUris.cshtml`
- `src/Yavsc.Org/Views/Client/EditScopes.cshtml`
- `src/Yavsc.Org/Views/Client/EditGrantTypes.cshtml`
- `src/Yavsc.Org/Views/Client/EditCorsOrigins.cshtml`
- `src/Yavsc.Org/Views/Client/EditIdPRestrictions.cshtml`
- `src/Yavsc.Org/Views/Client/EditClaims.cshtml`
- `src/Yavsc.Org/Views/Client/EditProperties.cshtml`
- `src/Yavsc.Org/Views/Client/EditSecrets.cshtml`
- `src/Yavsc.Org/Views/Client/_EditableStringList.cshtml`
  — partial consumed by the single-string-field collection pages.

Files modified:

- `src/Yavsc.Org/Controllers/Administration/ClientController.cs`
  — `class` → `partial class`; the `Edit(int id)` GET now uses
  `LoadClientAsync` to load all navigations (so the new Edit.cshtml
  can render counts in its nav links).
- `src/Yavsc.Org/Views/Client/Edit.cshtml`
  — significantly enriched: nav links to the 9 sub-pages, all the
  scalar fields split into fieldsets (Security, Logout, Tokens,
  Device / CIBA, Tokens-extra), ClientId / Id hidden.

### Known residual compile errors (4 errors total)

Paul is fixing these in a debug session. The structure is sound; the
errors are missing properties on the `Client` entity, a Razor
nullable quirk, and a `Localizer` injection miss.

1. `Edit.cshtml:249` — `PairwiseSubjectSalt` doesn't exist on
   `IdentityServer8.EntityFramework.Entities.Client`. **Fix**: drop
   the field from Edit.cshtml; IdentityServer8 likely uses a
   different property name (e.g. on a related entity) or doesn't
   expose it.
2. `Edit.cshtml:221` — `CibaLifetime` doesn't exist on `Client`.
   **Fix**: same as above. CIBA flow may be configured elsewhere
   (resource-level) or via a different property.
3. `ClientController.Collections.cs` lines 181, 217, 253, 304 —
   `Localizer` is not available in the partial class. **Fix**: inject
   `IStringLocalizer<ClientController>` via the constructor, or
   inline the strings ("BothTypeAndValueRequired", "KeyRequired",
   "ValueRequired", "SecretValueRequired").
4. `EditSecrets.cshtml:44` — `s.Expiration?.ToString("u")` on a
   `DateTime?`. **Fix**: just `s.Expiration?.ToString("u")` works
   if you write `s.Expiration.Value.ToString("u")`, or use
   `(s.Expiration is null ? "" : s.Expiration.Value.ToString("u"))`,
   or `s.Expiration?.ToString("u") ?? string.Empty`.

### Suggested next session

Once the 4 compile errors are fixed and the pages render:

1. Smoke test by logging in as admin, hitting `/Client/Edit/1`,
   then each `Edit*/1` page, and verifying add/remove POSTs.
2. Add a confirmation prompt (or 2-step form) for Remove actions —
   removing a Redirect URI is destructive and one click is too easy.
3. Wire up some collection-level validation (e.g. redirect URI must
   be a valid URL) at the controller level.
4. Add tests — the project doesn't have MVC test infrastructure
   today; consider adding a `Yavsc.Org.Tests` project that drives
   the controller via `WebApplicationFactory<Program>`.

