using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Interfaces
{
    public interface IViewFactory
    {
        void Register<TViewModel, TView>()
            where TViewModel : class, IModelViewModel
            where TView : Page;

        Page Resolve<TViewModel>(Action<TViewModel> setStateAction = null)
            where TViewModel : class, IModelViewModel;

        Page Resolve<TViewModel>(out TViewModel viewModel, Action<TViewModel> setStateAction = null)
            where TViewModel : class, IModelViewModel;

        Page Resolve<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IModelViewModel;
    }
}
