using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Threading;

namespace PostIt.Views;

public partial class MainView : UserControl
{
    private IInputPane? _inputPane;
    private IInsetsManager? _insetsManager;
    private Thickness _safeArea;
    private Rect? _keyboardRect;
    private IInputElement? _previousFocus;
    internal bool IsSoftKeyboardOpen { get; private set; }

    public MainView()
    {
        InitializeComponent();
        AuthenticationModal.PropertyChanged += (_, e) =>
        {
            if (e.Property != IsVisibleProperty) return;
            if (AuthenticationModal.IsVisible)
            {
                _previousFocus = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
                NavRoot.IsEnabled = false;
                Dispatcher.UIThread.Post(FocusAuthentication, DispatcherPriority.Input);
            }
            else
            {
                NavRoot.IsEnabled = true;
                var previousFocus = _previousFocus;
                _previousFocus = null;
                Dispatcher.UIThread.Post(() =>
                {
                    if (!AuthenticationModal.IsVisible)
                        previousFocus?.Focus();
                }, DispatcherPriority.Input);
            }
        };
        LoginButton.PropertyChanged += (_, e) =>
        {
            if (e.Property == IsEnabledProperty && LoginButton.IsEnabled && AuthenticationModal.IsVisible)
                Dispatcher.UIThread.Post(FocusAuthentication, DispatcherPriority.Input);
        };
        Loaded += (_, _) =>
        {
            Dispatcher.UIThread.Post(FocusAuthentication, DispatcherPriority.Input);
        };
        NavRoot.IsEnabled = !AuthenticationModal.IsVisible;
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

    private void FocusAuthentication()
    {
        if (AuthenticationModal.IsVisible)
            LoginButton.Focus();
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
