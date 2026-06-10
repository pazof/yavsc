using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PostIt.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    
    /// <summary>
    /// Gets if the user can navigate to the next page
    /// </summary>
    public abstract bool CanNavigateNext { get; protected set; }

    /// <summary>
    /// Gets if the user can navigate to the previous page
    /// </summary>
    public abstract bool CanNavigatePrevious { get; protected set; }

}
