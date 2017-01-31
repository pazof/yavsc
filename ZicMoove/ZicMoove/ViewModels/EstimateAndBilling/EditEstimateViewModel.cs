using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;

namespace ZicMoove.ViewModels.EstimateAndBilling
{
    using Model;
    using Model.Workflow;
    using Model.Social;
    using Validation;
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
        public BookQuery Query { get { return Data.Query; } }

        [JsonIgnore]
        public FormattedString FormattedTotal
        {
            get
            {
                /* 
                OnPlatform<Font> lfs = (OnPlatform<Font>)App.Current.Resources["MediumFontSize"];
                */
                OnPlatform<double> mfs = (OnPlatform < double > ) App.Current.Resources["MediumFontSize"];
                Color  etc = (Color) App.Current.Resources["EmphasisTextColor"];

                return new FormattedString
                {
                    Spans = {
                        new Span { Text = "Total TTC: " },
                        new Span { Text = Data.Total.ToString(),
                            ForegroundColor = etc,
                            FontSize = mfs },
                        new Span { Text = "€", FontSize = mfs }
                    }
                };
            }
        }
    }
}
