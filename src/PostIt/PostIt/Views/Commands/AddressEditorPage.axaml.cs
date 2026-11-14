using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using PostIt.ViewModels.Commands;

namespace PostIt.Views.Commands;

public partial class AddressEditorPage : ContentPage
{
    private const double DefaultLatitude = 48.8566;
    private const double DefaultLongitude = 2.3522;
    private const int DefaultZoomLevel = 4;
    private const int SelectedZoomLevel = 13;

    private MapControl? _locationMap;
    private MemoryLayer? _selectionLayer;
    private PhysicalAddressViewModel? _currentViewModel;

    public AddressEditorPage()
    {
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
        AttachViewModel(DataContext as PhysicalAddressViewModel);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        AttachViewModel(DataContext as PhysicalAddressViewModel);
        CenterFromViewModel();
    }

    private async void OnMapTapped(object? sender, MapEventArgs e)
    {
        if (DataContext is not PhysicalAddressViewModel vm)
            return;

        var (longitude, latitude) = SphericalMercator.ToLonLat(e.WorldPosition.X, e.WorldPosition.Y);
        vm.ApplyLocationFromMap(latitude, longitude);
        UpdateMarkerFromViewModel();
        CenterMap(latitude, longitude, zoomLevel: SelectedZoomLevel);
        await TryResolveAddressAsync(vm, latitude, longitude);
    }

    private async void OnUseCurrentLocationClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PhysicalAddressViewModel vm)
            return;

        await vm.UseCurrentLocationCommand.ExecuteAsync(null);
        UpdateMarkerFromViewModel();
        CenterFromViewModel();

        if (vm.Latitude.HasValue && vm.Longitude.HasValue)
            await vm.TryResolveAddressAsync(vm.Latitude.Value, vm.Longitude.Value);
    }

    private async void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Application.Current is App app)
            await app.GoBackAsync();
    }

    private async void OnValidateClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Application.Current is App app)
            await app.GoBackAsync();
    }

    private void AttachViewModel(PhysicalAddressViewModel? vm)
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
        if (e.PropertyName == nameof(PhysicalAddressViewModel.Latitude)
            || e.PropertyName == nameof(PhysicalAddressViewModel.Longitude))
        {
            UpdateMarkerFromViewModel();
        }
    }

    private void CenterFromViewModel()
    {
        if (DataContext is not PhysicalAddressViewModel vm)
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

        if (DataContext is not PhysicalAddressViewModel vm
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

    private Task TryResolveAddressAsync(PhysicalAddressViewModel vm, double latitude, double longitude)
    {
        return vm.TryResolveAddressAsync(latitude, longitude);
    }
}
