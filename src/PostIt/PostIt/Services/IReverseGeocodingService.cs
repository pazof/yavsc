using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

public interface IReverseGeocodingService
{
    Task<string?> TryResolveAddressAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}