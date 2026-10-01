using System.Threading.Tasks;
using PostIt.ViewModels;
using Avalonia.Controls;

namespace PostIt;

internal interface IVMPusherApp
{
    Task<Page> PushPageAsync(ViewModelBase viewModel);
}
