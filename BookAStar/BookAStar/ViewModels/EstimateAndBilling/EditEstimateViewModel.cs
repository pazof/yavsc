﻿using System.Collections.Generic;
using BookAStar.Model.Workflow;
using System.Collections.ObjectModel;
using BookAStar.Model;
using Xamarin.Forms;
using BookAStar.Data;
using Newtonsoft.Json;
using System.Linq;
using BookAStar.ViewModels.Validation;
using System.ComponentModel;

namespace BookAStar.ViewModels.EstimateAndBilling
{
    public class EditEstimateViewModel : EditingViewModel<Estimate>
    {

        /// <summary>
        /// Builds a new view model on estimate,
        /// sets <c>Data</c> with given value parameter
        /// </summary>
        /// <param name="data"></param>
        /// <param name="localState"></param>
        public EditEstimateViewModel(Estimate data) : base(data)
        {
            SyncData();
        }
        public override void OnViewAppearing()
        {
            base.OnViewAppearing();
            SyncData();
        }
        /// <summary>
        /// Called to synchronyze this view on target model,
        /// at accepting a new representation for this model
        /// </summary>
        private void SyncData()
        {
            if (Data.AttachedFiles == null) Data.AttachedFiles = new List<string>();
            if (Data.AttachedGraphics == null) Data.AttachedGraphics = new List<string>();
            if (Data.Bill == null) Data.Bill = new List<BillingLine>();
            AttachedFiles = new ObservableCollection<string>(Data.AttachedFiles);
            AttachedGraphicList = new ObservableCollection<string>(Data.AttachedGraphics);
            Bill = new ObservableCollection<BillingLineViewModel>(Data.Bill.Select(
                l => new BillingLineViewModel(l)
                ));
            Bill.CollectionChanged += Bill_CollectionChanged;
            Title = Data.Title;
            Description = Data.Description;
            NotifyPropertyChanged("FormattedTotal");
            NotifyPropertyChanged("Query");
            NotifyPropertyChanged("CLient");
            NotifyPropertyChanged("ModelState");
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName.StartsWith("Data"))
            {
                SyncData();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bill_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Data.Bill = Bill.Select(l => l.Data).ToList();
            NotifyPropertyChanged("FormattedTotal");
            NotifyPropertyChanged("Bill");
            NotifyPropertyChanged("ViewModelState");
        }

        [JsonIgnore]
        public ObservableCollection<string> AttachedFiles
        {
            get; protected set;
        }

        [JsonIgnore]
        public ObservableCollection<string> AttachedGraphicList
        {
            get; protected set;
        }

        [JsonIgnore]
        public ObservableCollection<BillingLineViewModel> Bill
        {
            get; protected set;
        }


       
        [JsonIgnore]
        private string description;
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                SetProperty<string>(ref description, value);
                Data.Description = description;
            }
        }

        private string title;
        [JsonIgnore]
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                SetProperty<string>(ref title, value, "Title");
                Data.Title = title;
            }
        }

        [JsonIgnore]
        public ClientProviderInfo Client {  get { return Data.Client; } }

        [JsonIgnore]
        public BookQueryData Query { get { return Data.Query; } }

        [JsonIgnore]
        public FormattedString FormattedTotal
        {
            get
            {
                OnPlatform<Font> lfs = (OnPlatform<Font>)App.Current.Resources["MediumFontSize"];
                Color  etc = (Color) App.Current.Resources["EmphasisTextColor"];

                return new FormattedString
                {

                    Spans = {
                        new Span { Text = "Total TTC: " },
                        new Span { Text = Data.Total.ToString(),
                            ForegroundColor = etc,
                            FontSize = (double) lfs.Android.FontSize },
                        new Span { Text = "€", FontSize = (double) lfs.Android.FontSize }
                    }
                };
            }
        }
    }
}
