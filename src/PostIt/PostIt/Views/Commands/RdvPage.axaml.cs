using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using PostIt.ViewModels.Commands;

namespace PostIt.Views.Commands;

public partial class RdvPage : ContentPage
{
    private MapControl? _locationMap;

    public RdvPage()
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
        _locationMap.Map = map;
        _locationMap.MapTapped += OnMapTapped;

        DataContextChanged += OnDataContextChanged;
        CenterFromViewModel();
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        CenterFromViewModel();
    }

    private void OnMapTapped(object? sender, MapEventArgs e)
    {
        if (DataContext is not RdvViewModel vm)
            return;

        var (longitude, latitude) = SphericalMercator.ToLonLat(e.WorldPosition.X, e.WorldPosition.Y);
        vm.ApplyLocationFromMap(latitude, longitude);
        CenterMap(latitude, longitude, zoomLevel: 13);
    }

    private void CenterFromViewModel()
    {
        if (DataContext is not RdvViewModel vm)
        {
            CenterMap(48.8566, 2.3522, zoomLevel: 4);
            return;
        }

        if (vm.Latitude.HasValue && vm.Longitude.HasValue)
        {
            CenterMap(vm.Latitude.Value, vm.Longitude.Value, zoomLevel: 13);
            return;
        }

        CenterMap(48.8566, 2.3522, zoomLevel: 4);
    }

    private void CenterMap(double latitude, double longitude, int zoomLevel)
    {
        if (_locationMap?.Map is null)
            return;

        var (x, y) = SphericalMercator.FromLonLat(longitude, latitude);
        _locationMap.Map.Navigator.CenterOn(x, y);
        _locationMap.Map.Navigator.ZoomToLevel(zoomLevel);
    }
}
