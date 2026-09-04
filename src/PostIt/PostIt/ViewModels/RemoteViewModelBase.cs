using System.Threading.Tasks;

namespace PostIt.ViewModels;

public abstract class RemoteViewModelBase : ViewModelBase
{
     public abstract Task LoadAsync();


}
