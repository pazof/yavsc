using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Locations;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using PostIt.Services;

namespace PostIt.Android.Services;

internal static class AndroidCurrentLocationProvider
{
    public static async Task<CurrentLocationResult> TryGetCurrentLocationAsync(CancellationToken cancellationToken)
    {
        var activity = MainActivity.Current;
        if (activity is null)
        {
            return CurrentLocationResult.Unavailable("L'activité Android n'est pas encore prête.");
        }

        var permissionGranted = await LocationPermissionBroker.EnsureGrantedAsync(activity, cancellationToken).ConfigureAwait(false);
        if (!permissionGranted)
        {
            return CurrentLocationResult.PermissionDenied();
        }

        var locationManager = activity.GetSystemService(global::Android.Content.Context.LocationService) as LocationManager;
        if (locationManager is null)
        {
            return CurrentLocationResult.Unavailable("Le service de localisation Android est indisponible.");
        }

        var location = locationManager.GetProviders(enabledOnly: true)?
            .Select(provider => locationManager.GetLastKnownLocation(provider))
            .Where(candidate => candidate is not null)
            .OrderByDescending(candidate => candidate!.Time)
            .ThenBy(candidate => candidate!.Accuracy)
            .FirstOrDefault();

        if (location is null)
        {
            return CurrentLocationResult.Unavailable("Aucune position n'est disponible. Activez la localisation du système puis réessayez.");
        }

        return CurrentLocationResult.Success(location.Latitude, location.Longitude);
    }

    public static bool HandlePermissionResult(int requestCode, Permission[]? grantResults)
        => LocationPermissionBroker.HandleResult(requestCode, grantResults);

    private static class LocationPermissionBroker
    {
        private const int RequestCode = 4042;
        private static readonly string[] RequestedPermissions =
        {
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.AccessCoarseLocation,
        };

        private static readonly object SyncRoot = new();
        private static TaskCompletionSource<bool>? _pendingRequest;

        public static Task<bool> EnsureGrantedAsync(Activity activity, CancellationToken cancellationToken)
        {
            if (HasLocationPermission(activity))
            {
                return Task.FromResult(true);
            }

            lock (SyncRoot)
            {
                if (_pendingRequest is null)
                {
                    _pendingRequest = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    ActivityCompat.RequestPermissions(activity, RequestedPermissions, RequestCode);
                }

                if (!cancellationToken.CanBeCanceled)
                {
                    return _pendingRequest.Task;
                }

                return WaitAsync(_pendingRequest.Task, cancellationToken);
            }
        }

        public static bool HandleResult(int requestCode, Permission[]? grantResults)
        {
            if (requestCode != RequestCode)
            {
                return false;
            }

            var granted = grantResults is { Length: > 0 } && grantResults.All(result => result == Permission.Granted);
            TaskCompletionSource<bool>? pendingRequest;
            lock (SyncRoot)
            {
                pendingRequest = _pendingRequest;
                _pendingRequest = null;
            }

            pendingRequest?.TrySetResult(granted);
            return true;
        }

        private static bool HasLocationPermission(Activity activity)
        {
            return ContextCompat.CheckSelfPermission(activity, Manifest.Permission.AccessFineLocation) == Permission.Granted
                || ContextCompat.CheckSelfPermission(activity, Manifest.Permission.AccessCoarseLocation) == Permission.Granted;
        }

        private static async Task<bool> WaitAsync(Task<bool> task, CancellationToken cancellationToken)
        {
            using var registration = cancellationToken.Register(() =>
            {
                lock (SyncRoot)
                {
                    _pendingRequest?.TrySetCanceled(cancellationToken);
                    _pendingRequest = null;
                }
            });

            return await task.ConfigureAwait(false);
        }
    }
}
