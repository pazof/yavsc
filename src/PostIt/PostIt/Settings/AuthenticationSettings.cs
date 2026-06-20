using CommunityToolkit.Mvvm.ComponentModel;
using System;

public partial class AuthenticationSettings : ObservableObject
{

    [ObservableProperty]
    public partial string Authority { get; set; }

    [ObservableProperty]
    public partial string ClientId { get; set; }

}