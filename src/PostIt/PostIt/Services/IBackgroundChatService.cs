using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Services;

public interface IBackgroundChatService
{
    Task SynchronizeAsync(YavscApiClient api, bool enabled, CancellationToken ct = default);
    Task DisableAsync(YavscApiClient api, CancellationToken ct = default);
}
