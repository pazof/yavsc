﻿using BookAStar.Model;
using BookAStar.Model.Workflow;
using BookAStar.ViewModels;
using BookAStar.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XLabs.Forms.Services;
using XLabs.Ioc;
using XLabs.Platform.Services;

namespace BookAStar.Pages
{
    public partial class EditEstimatePage : ContentPage
    {

        public EditEstimatePage(EstimateViewModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            // FIXME WAZA
           if (BindingContext != null) { 
                mdview.Markdown = ((EstimateViewModel)BindingContext).Description;
            }
        }

        protected void OnDescriptionChanged (object sender, EventArgs e)
        {
            // FIXME Why the Binding don't work?
            ((EstimateViewModel)BindingContext).Description = mdview.Markdown;
            InvalidateMeasure();
        }

        protected void OnNewCommanLine(object sender, EventArgs e)
        {
            var com = new BillingLine();
            App.CurrentApp.NavigationService.NavigateTo<EditBillingLinePage>(
                true,
                new object[] { new BillingLineViewModel(((EstimateViewModel)this.BindingContext).Data,com) } );
        }
       
    }
}