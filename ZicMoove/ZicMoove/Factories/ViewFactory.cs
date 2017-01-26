﻿using ZicMoove.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ZicMoove.Factories
{
    public class ViewFactory : IViewFactory
    {
        private readonly IDictionary<Type, Type> _map = new Dictionary<Type, Type>();
        private readonly IComponentContext _componentContext;

        public ViewFactory(IComponentContext componentContext)
        {
            _componentContext = componentContext;
        }

        public void Register<TViewModel, TView>()
            where TViewModel : class, IModelViewModel
            where TView : Page
        {
            _map[typeof(TViewModel)] = typeof(TView);
        }

        public Page Resolve<TViewModel>(Action<TViewModel> setStateAction = null) where TViewModel : class, IModelViewModel
        {
            TViewModel viewModel;
            return Resolve<TViewModel>(out viewModel, setStateAction) ;
        }

        public Page Resolve<TViewModel>(out TViewModel viewModel, Action<TViewModel> setStateAction = null)
            where TViewModel : class, IModelViewModel
        {
            viewModel = _componentContext.Resolve<TViewModel>();

            var viewType = _map[typeof(TViewModel)];
            var view = _componentContext.Resolve(viewType) as Page;

            if (setStateAction != null)
                viewModel.SetState(setStateAction);

            view.BindingContext = viewModel;
            return view;
        }

        public Page Resolve<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IModelViewModel
        {
            var viewType = _map[typeof(TViewModel)];
            var view = _componentContext.Resolve(viewType) as Page;
            view.BindingContext = viewModel;
            return view;
        }
    }
}
