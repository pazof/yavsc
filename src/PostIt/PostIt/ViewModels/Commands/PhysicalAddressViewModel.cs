using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PostIt.Services;

namespace PostIt.ViewModels.Commands;

public partial class PhysicalAddressViewModel : ViewModelBase
{
    private readonly IReverseGeocodingService _reverseGeocodingService;
    private CancellationTokenSource? _reverseGeocodeCts;
    private double? _lastResolvedLatitude;
    private double? _lastResolvedLongitude;
    private string? _lastResolvedAddress;

    private const double ReverseGeocodingCacheToleranceDegrees = 0.0001;
    private static readonly TimeSpan ReverseGeocodingDebounce = TimeSpan.FromMilliseconds(350);

    [ObservableProperty]
    public partial string Address { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SuggestedAddress { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsResolvingAddress { get; set; }

    [ObservableProperty]
    public partial double? Latitude { get; set; }

    [ObservableProperty]
    public partial double? Longitude { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasSuggestedAddress => !string.IsNullOrWhiteSpace(SuggestedAddress);
    public bool HasSuggestedAddressPanel => HasSuggestedAddress || IsResolvingAddress;
    public bool CanUseCurrentLocation => !IsBusy;

    public override bool CanNavigateNext { get; protected set; }
    public override bool CanNavigatePrevious { get; protected set; } = true;

    public PhysicalAddressViewModel(IReverseGeocodingService reverseGeocodingService)
    {
        _reverseGeocodingService = reverseGeocodingService ?? throw new ArgumentNullException(nameof(reverseGeocodingService));
    }

    public void ApplyLocationFromMap(double latitude, double longitude)
    {
        Latitude = Math.Round(latitude, 6);
        Longitude = Math.Round(longitude, 6);
    }

    public void NotifyReverseGeocodingStarted()
    {
        IsResolvingAddress = true;
    }

    public void NotifyReverseGeocodingUnavailable()
    {
        IsResolvingAddress = false;
    }

    public void ApplyResolvedAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;

        var trimmedAddress = address.Trim();
        if (string.IsNullOrWhiteSpace(Address))
        {
            Address = trimmedAddress;
            SuggestedAddress = string.Empty;
            IsResolvingAddress = false;
            return;
        }

        if (string.Equals(Address.Trim(), trimmedAddress, StringComparison.Ordinal))
        {
            SuggestedAddress = string.Empty;
            IsResolvingAddress = false;
            return;
        }

        SuggestedAddress = trimmedAddress;
        IsResolvingAddress = false;
    }

    [RelayCommand(CanExecute = nameof(HasSuggestedAddress))]
    private void ApplySuggestedAddress()
    {
        if (string.IsNullOrWhiteSpace(SuggestedAddress))
            return;

        Address = SuggestedAddress.Trim();
        SuggestedAddress = string.Empty;
        IsResolvingAddress = false;
    }

    [RelayCommand(CanExecute = nameof(CanUseCurrentLocation))]
    private async Task UseCurrentLocationAsync()
    {
        if (!CanUseCurrentLocation)
            return;

        IsBusy = true;
        try
        {
            var result = await Platform.TryGetCurrentLocationAsync(default).ConfigureAwait(true);
            if (!result.IsSuccess || !result.Latitude.HasValue || !result.Longitude.HasValue)
            {
                return;
            }

            Latitude = result.Latitude.Value;
            Longitude = result.Longitude.Value;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task TryResolveAddressAsync(double latitude, double longitude)
    {
        if (_lastResolvedLatitude.HasValue
            && _lastResolvedLongitude.HasValue
            && AreCoordinatesClose(_lastResolvedLatitude.Value, _lastResolvedLongitude.Value, latitude, longitude)
            && !string.IsNullOrWhiteSpace(_lastResolvedAddress))
        {
            ApplyResolvedAddress(_lastResolvedAddress);
            return;
        }

        _reverseGeocodeCts?.Cancel();
        _reverseGeocodeCts?.Dispose();
        _reverseGeocodeCts = new CancellationTokenSource();
        var cancellationToken = _reverseGeocodeCts.Token;

        try
        {
            NotifyReverseGeocodingStarted();
            await Task.Delay(ReverseGeocodingDebounce, cancellationToken).ConfigureAwait(true);

            var resolved = await _reverseGeocodingService
                .TryResolveAddressAsync(latitude, longitude, cancellationToken)
                .ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(resolved))
            {
                _lastResolvedLatitude = latitude;
                _lastResolvedLongitude = longitude;
                _lastResolvedAddress = resolved;
                ApplyResolvedAddress(resolved);
                return;
            }

            NotifyReverseGeocodingUnavailable();
        }
        catch (OperationCanceledException)
        {
            IsResolvingAddress = false;
        }
    }

    partial void OnSuggestedAddressChanged(string value)
    {
        OnPropertyChanged(nameof(HasSuggestedAddress));
        OnPropertyChanged(nameof(HasSuggestedAddressPanel));
        ApplySuggestedAddressCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsResolvingAddressChanged(bool value)
    {
        OnPropertyChanged(nameof(HasSuggestedAddressPanel));
    }

    public static bool AreCoordinatesClose(
        double latitudeA,
        double longitudeA,
        double latitudeB,
        double longitudeB)
    {
        return Math.Abs(latitudeA - latitudeB) <= ReverseGeocodingCacheToleranceDegrees
            && Math.Abs(longitudeA - longitudeB) <= ReverseGeocodingCacheToleranceDegrees;
    }
}
