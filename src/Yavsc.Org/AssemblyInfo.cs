using Microsoft.Extensions.Localization;
using System.Runtime.CompilerServices;

[assembly: RootNamespace("Yavsc")]

// Expose internals to the Yavsc.Org.Tests project so unit tests can
// reach the signing-credential loader (LoadSigningCredentials / kid
// derivation) without going through the full IdentityServer boot.
[assembly: InternalsVisibleTo("Yavsc.Org.Tests")]
