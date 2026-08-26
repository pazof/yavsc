using System.Threading;
using System.Threading.Tasks;

namespace PostIt.Droid.Services;

public static class OidcCallbackManager
{
    private static TaskCompletionSource<string>? _tcs;

    public static Task<string> RegisterCallback(CancellationToken cancellationToken)
    {
        _tcs = new TaskCompletionSource<string>();
        cancellationToken.Register(() => _tcs.TrySetCanceled());
        return _tcs.Task;
    }

    public static void SetResult(string url)
    {
        _tcs?.TrySetResult(url);
    }
}
