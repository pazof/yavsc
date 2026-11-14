using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PostIt.Models;
using PostIt.Services;

namespace PostIt.Views;

public partial class MainView : UserControl
{
    private IInputPane? _inputPane;
    private IInsetsManager? _insetsManager;
    private Thickness _safeArea;
    private Rect? _keyboardRect;
    private IInputElement? _previousFocus;
    private bool _toastIsVisible;
    private readonly NotificationService _notificationService = NotificationService.Instance;
    internal bool IsSoftKeyboardOpen { get; private set; }

    public MainView()
    {
        InitializeComponent();
        _notificationService.NotificationRequested += OnNotificationRequested;

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

        Loaded += (_, _) => Dispatcher.UIThread.Post(FocusAuthentication, DispatcherPriority.Input);

        NotificationModalCloseButton.Click += (_, _) => HideNotificationModal();
        NotificationModalActionButton.Click += (_, _) => HideNotificationModal(executeAction: true);

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

    private void OnNotificationRequested(object? sender, NotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            HideToast();
            HideNotificationModal();
            return;
        }

        if (request.Severity is StatusSeverity.Warning or StatusSeverity.Error)
        {
            ShowNotificationModal(request.Message, request.Severity, request.ActionLabel, request.Action);
            return;
        }

        ShowToast(request.Message, request.Severity, request.ActionLabel, request.Action);
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
        var overlap = keyboardRect is { Height: > 0 } rect && bottom is { } point
            ? Math.Clamp(point.Y - rect.Top, 0, Bounds.Height)
            : 0;

        Padding = new Thickness(_safeArea.Left, _safeArea.Top, _safeArea.Right,
            Math.Max(_safeArea.Bottom, overlap));
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
    
        if (DataContext is ViewModels.BlogsViewModel vm && !vm.IsLoaded)
            vm.RefreshAsync().Wait();
    }

    private void NotificationToast_PointerPressed(object? sender, PointerPressedEventArgs e)
        => HideToast();

    private void NotificationModalCloseButton_Click(object? sender, RoutedEventArgs e)
        => HideNotificationModal();

    private void NotificationModalActionButton_Click(object? sender, RoutedEventArgs e)
        => HideNotificationModal(executeAction: true);

    internal void ShowToast(string message, StatusSeverity severity = StatusSeverity.Info, string? actionLabel = null, Action? action = null)
    {
        var glyph = severity switch
        {
            StatusSeverity.Error => "!",
            StatusSeverity.Warning => "~",
            _ => "✓"
        };

        var background = severity switch
        {
            StatusSeverity.Error => Color.Parse("#7F1D1D"),
            StatusSeverity.Warning => Color.Parse("#7C4A03"),
            _ => Color.Parse("#E8F0FE")
        };

        var borderBrush = severity switch
        {
            StatusSeverity.Error => Color.Parse("#C62828"),
            StatusSeverity.Warning => Color.Parse("#E6A700"),
            _ => Color.Parse("#5B8DEF")
        };

        var foreground = severity switch
        {
            StatusSeverity.Error => Color.Parse("#E1F0F6"),
            StatusSeverity.Warning => Color.Parse("#EAEAEA"),
            _ => Color.Parse("#1E3A8A")
        };

        NotificationToastGlyph.Text = glyph;
        NotificationToastGlyph.Foreground = new SolidColorBrush(foreground);
        NotificationToastMessage.Text = message;
        NotificationToastMessage.Foreground = new SolidColorBrush(foreground);
        NotificationToast.Background = new SolidColorBrush(background);
        NotificationToast.BorderBrush = new SolidColorBrush(borderBrush);
        NotificationToast.IsVisible = true;
        _toastIsVisible = true;

        if (action is not null && !string.IsNullOrWhiteSpace(actionLabel))
        {
            NotificationModalActionButton.Content = actionLabel;
            NotificationModalActionButton.IsVisible = true;
            NotificationModalActionButton.Tag = action;
        }
        else
        {
            NotificationModalActionButton.Content = string.Empty;
            NotificationModalActionButton.IsVisible = false;
            NotificationModalActionButton.Tag = null;
        }

        if (severity is StatusSeverity.Warning or StatusSeverity.Error)
        {
            ShowNotificationModal(message, severity, actionLabel, action);
            return;
        }

        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(2800);
            if (_toastIsVisible)
                HideToast();
        });
    }

    internal void HideToast()
    {
        _toastIsVisible = false;
        NotificationToast.IsVisible = false;
    }

    internal void ShowNotificationModal(string message, StatusSeverity severity, string? actionLabel = null, Action? action = null)
    {
        var title = severity switch
        {
            StatusSeverity.Error => "Erreur",
            StatusSeverity.Warning => "Attention",
            _ => "Information"
        };

        NotificationModalTitle.Text = title;
        NotificationModalMessage.Text = message;

        if (action is not null && !string.IsNullOrWhiteSpace(actionLabel))
        {
            NotificationModalActionButton.Content = actionLabel;
            NotificationModalActionButton.IsVisible = true;
            NotificationModalActionButton.Tag = action;
        }
        else
        {
            NotificationModalActionButton.Content = string.Empty;
            NotificationModalActionButton.IsVisible = false;
            NotificationModalActionButton.Tag = null;
        }

        NotificationModal.IsVisible = true;
        HideToast();
    }

    internal void HideNotificationModal(bool executeAction = false)
    {
        NotificationModal.IsVisible = false;
        if (executeAction && NotificationModalActionButton.Tag is Action action)
        {
            NotificationModalActionButton.Tag = null;
            action();
        }
        else
        {
            NotificationModalActionButton.Tag = null;
        }
    }
}
