using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using PostIt.Services;
using PostIt.ViewModels.Commands;

namespace PostIt.Views.Commands;

public partial class RdvPage : ContentPage
{
    private const double DefaultLatitude = 48.8566;
    private const double DefaultLongitude = 2.3522;
    private const int DefaultZoomLevel = 4;
    private const int SelectedZoomLevel = 13;
    internal const double ReverseGeocodingCacheToleranceDegrees = 0.0001;
    private static readonly TimeSpan ReverseGeocodingDebounce = TimeSpan.FromMilliseconds(350);

    private MapControl? _locationMap;
    private MemoryLayer? _selectionLayer;
    private RdvViewModel? _currentViewModel;
    private readonly IReverseGeocodingService _reverseGeocodingService;
    private CancellationTokenSource? _reverseGeocodeCts;
    private double? _lastResolvedLatitude;
    private double? _lastResolvedLongitude;
    private string? _lastResolvedAddress;

    public RdvPage()
        : this(null)
    {
    }

    public RdvPage(IReverseGeocodingService? reverseGeocodingService)
    {
        _reverseGeocodingService = reverseGeocodingService ?? new NominatimReverseGeocodingService();
        InitializeComponent();
        InitializeMap();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitializeMap()
    {
        _locationMap = this.FindControl<MapControl>("LocationMap");
        if (_locationMap is null)
            return;

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        _selectionLayer = CreateSelectionLayer();
        map.Layers.Add(_selectionLayer);
        _locationMap.Map = map;
        _locationMap.MapTapped += OnMapTapped;

        DataContextChanged += OnDataContextChanged;
        AttachViewModel(DataContext as RdvViewModel);
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        AttachViewModel(DataContext as RdvViewModel);
        CenterFromViewModel();
    }

    private async void OnMapTapped(object? sender, MapEventArgs e)
    {
        if (DataContext is not RdvViewModel vm)
            return;

        var (longitude, latitude) = SphericalMercator.ToLonLat(e.WorldPosition.X, e.WorldPosition.Y);
        vm.ApplyLocationFromMap(latitude, longitude);
        UpdateMarkerFromViewModel();
        CenterMap(latitude, longitude, zoomLevel: SelectedZoomLevel);
        await TryResolveAddressAsync(vm, latitude, longitude);
    }

    private async void OnCenterCurrentLocationClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not RdvViewModel vm || !vm.CanUseCurrentLocation)
            return;

        await vm.UseCurrentLocationCommand.ExecuteAsync(null);
        UpdateMarkerFromViewModel();
        CenterFromViewModel();

        if (vm.Latitude.HasValue && vm.Longitude.HasValue)
            await TryResolveAddressAsync(vm, vm.Latitude.Value, vm.Longitude.Value);
    }

    private void AttachViewModel(RdvViewModel? vm)
    {
        if (_currentViewModel is not null)
            _currentViewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _currentViewModel = vm;

        if (_currentViewModel is not null)
            _currentViewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateMarkerFromViewModel();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RdvViewModel.Latitude)
            || e.PropertyName == nameof(RdvViewModel.Longitude))
        {
            UpdateMarkerFromViewModel();
        }
    }

    private void CenterFromViewModel()
    {
        if (DataContext is not RdvViewModel vm)
        {
            CenterMap(DefaultLatitude, DefaultLongitude, zoomLevel: DefaultZoomLevel);
            return;
        }

        if (vm.Latitude.HasValue && vm.Longitude.HasValue)
        {
            CenterMap(vm.Latitude.Value, vm.Longitude.Value, zoomLevel: SelectedZoomLevel);
            return;
        }

        CenterMap(DefaultLatitude, DefaultLongitude, zoomLevel: DefaultZoomLevel);
    }

    private void CenterMap(double latitude, double longitude, int zoomLevel)
    {
        if (_locationMap?.Map is null)
            return;

        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        _locationMap.Map.Navigator.CenterOn(x, y);
        _locationMap.Map.Navigator.ZoomToLevel(zoomLevel);
    }

    private void UpdateMarkerFromViewModel()
    {
        if (_selectionLayer is null)
            return;

        if (DataContext is not RdvViewModel vm
            || !vm.Latitude.HasValue
            || !vm.Longitude.HasValue)
        {
            _selectionLayer.Features = Enumerable.Empty<IFeature>();
            _locationMap?.RefreshData(ChangeType.Discrete);
            return;
        }

        var (x, y) = SphericalMercator.FromLonLat(vm.Longitude.Value, vm.Latitude.Value);
        _selectionLayer.Features = new IFeature[] { new PointFeature(x, y) };
        _locationMap?.RefreshData(ChangeType.Discrete);
    }

    private static MemoryLayer CreateSelectionLayer()
    {
        return new MemoryLayer("selected-location")
        {
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Ellipse,
                Fill = new Brush(Color.Crimson),
                Outline = new Pen(Color.White, 2),
                SymbolScale = 0.9,
            },
            Features = Enumerable.Empty<IFeature>(),
        };
    }

    private async Task TryResolveAddressAsync(RdvViewModel vm, double latitude, double longitude)
    {
        if (_lastResolvedLatitude.HasValue
            && _lastResolvedLongitude.HasValue
            && AreCoordinatesClose(_lastResolvedLatitude.Value, _lastResolvedLongitude.Value, latitude, longitude)
            && !string.IsNullOrWhiteSpace(_lastResolvedAddress))
        {
            vm.ApplyResolvedAddress(_lastResolvedAddress);
            return;
        }

        _reverseGeocodeCts?.Cancel();
        _reverseGeocodeCts?.Dispose();
        _reverseGeocodeCts = new CancellationTokenSource();
        var cancellationToken = _reverseGeocodeCts.Token;

        try
        {
            vm.NotifyReverseGeocodingStarted();
            await Task.Delay(ReverseGeocodingDebounce, cancellationToken).ConfigureAwait(true);

            var resolved = await _reverseGeocodingService
                .TryResolveAddressAsync(latitude, longitude, cancellationToken)
                .ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(resolved))
            {
                _lastResolvedLatitude = latitude;
                _lastResolvedLongitude = longitude;
                _lastResolvedAddress = resolved;
                vm.ApplyResolvedAddress(resolved);
                return;
            }

            vm.NotifyReverseGeocodingUnavailable();
        }
        catch (OperationCanceledException)
        {
            vm.IsResolvingAddress = false;
        }
    }

    internal static bool AreCoordinatesClose(
        double latitudeA,
        double longitudeA,
        double latitudeB,
        double longitudeB)
    {
        return Math.Abs(latitudeA - latitudeB) <= ReverseGeocodingCacheToleranceDegrees
            && Math.Abs(longitudeA - longitudeB) <= ReverseGeocodingCacheToleranceDegrees;
    }
}
