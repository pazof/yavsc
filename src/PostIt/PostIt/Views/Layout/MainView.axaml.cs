using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;

namespace PostIt.Views;

public partial class MainView : UserControl
{
    private IInputPane? _inputPane;
    private IInsetsManager? _insetsManager;
    private Thickness _safeArea;
    private Rect? _keyboardRect;
    internal bool IsSoftKeyboardOpen { get; private set; }

    public MainView()
    {
        InitializeComponent();
        TopLevel.SetAutoSafeAreaPadding(this, false);
        AttachedToVisualTree += (_, _) =>
        {
            var topLevel = TopLevel.GetTopLevel(this);
            _inputPane = topLevel?.InputPane;
            _insetsManager = topLevel?.InsetsManager;
            if (_insetsManager is not null)
            {
                _insetsManager.SafeAreaChanged += SafeAreaChanged;
                _safeArea = _insetsManager.SafeAreaPadding;
            }
            if (_inputPane is not null)
            {
                _inputPane.StateChanged += InputPaneStateChanged;
                ApplyKeyboardOcclusion(_inputPane.State == InputPaneState.Open ? _inputPane.OccludedRect : null);
            }
            ApplyKeyboardOcclusion(_keyboardRect);
        };
        DetachedFromVisualTree += (_, _) =>
        {
            if (_inputPane is not null)
                _inputPane.StateChanged -= InputPaneStateChanged;
            _inputPane = null;
            if (_insetsManager is not null)
                _insetsManager.SafeAreaChanged -= SafeAreaChanged;
            _insetsManager = null;
        };
        SizeChanged += (_, _) => ApplyKeyboardOcclusion(_keyboardRect);
    }

    private void InputPaneStateChanged(object? sender, InputPaneStateEventArgs e)
        => ApplyKeyboardOcclusion(e.NewState == InputPaneState.Open ? e.EndRect : null);

    private void SafeAreaChanged(object? sender, SafeAreaChangedArgs e)
        => ApplySafeArea(e.SafeAreaPadding);

    internal void ApplySafeArea(Thickness safeArea)
    {
        _safeArea = safeArea;
        ApplyKeyboardOcclusion(_keyboardRect);
    }

    internal void ApplyKeyboardOcclusion(Rect? keyboardRect)
    {
        _keyboardRect = keyboardRect;
        IsSoftKeyboardOpen = keyboardRect is { Height: > 0 };
        var topLevel = TopLevel.GetTopLevel(this);
        var bottom = topLevel is null ? null : this.TranslatePoint(new Point(0, Bounds.Height), topLevel);
        // Subtract only actual overlap: AdjustResize may already have removed the keyboard area.
        var overlap = keyboardRect is { Height: > 0 } rect && bottom is { } point
            ? Math.Clamp(point.Y - rect.Top, 0, Bounds.Height)
            : 0;
        Padding = new Thickness(_safeArea.Left, _safeArea.Top, _safeArea.Right,
            Math.Max(_safeArea.Bottom, overlap));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is ViewModels.BlogsViewModel vm)
        {
            if (!vm.IsLoaded)
            {
                vm.RefreshAsync().Wait();
            }
        }
    }
}
