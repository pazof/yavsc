using Avalonia.Controls;
using Avalonia.Styling;
using PostIt.ViewModels;
using System;
using System.Threading.Tasks;

namespace PostIt.Views;

public partial class MainWindow : Window
{
    internal Settings Settings { get; private set; }

    public MainWindow()
    {

        InitializeComponent();

        this.Settings = new Settings();


    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is MainViewModel vm)
        {
            Task.Run(async () => await this.Settings.Load(this.StorageProvider)).Wait();
            this.RequestedThemeVariant = Settings.DarkMode ? ThemeVariant.Dark : ThemeVariant.Light;
            MainView.DataContext = new MainViewModel();
            vm.Settings.Load(this.StorageProvider).Wait();
        }
    }

}