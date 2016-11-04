using System.Collections.Generic;
using BookAStar.Model.Workflow;
using System.Collections.ObjectModel;
using BookAStar.Model;
using Xamarin.Forms;
using BookAStar.Data;
using Newtonsoft.Json;

namespace BookAStar.ViewModels
{
    public class EditEstimateViewModel : EditingViewModel
    {
        /// <summary>
        /// For deserialization
        /// </summary>
        public EditEstimateViewModel()
        {

        }
        /// <summary>
        /// Builds a new view model on estimate,
        /// sets <c>Data</c> with given value parameter
        /// </summary>
        /// <param name="data"></param>
        /// <param name="localState"></param>
        public EditEstimateViewModel(Estimate data, LocalState localState )
        {
            Data = data;
            State = localState;
        }

        private void Bill_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Data.Bill = Bill;
            NotifyPropertyChanged("FormattedTotal");
        }
        private Estimate data;
        public Estimate Data { get { return data; } set {
                data = value;
                if (data.AttachedFiles == null) data.AttachedFiles = new List<string>();
                if (data.AttachedGraphics == null) data.AttachedGraphics = new List<string>();
                if (data.Bill == null) data.Bill = new List<BillingLine>();
                AttachedFiles = new ObservableCollection<string>(data.AttachedFiles);
                AttachedGraphicList = new ObservableCollection<string>(data.AttachedGraphics);
                Bill = new ObservableCollection<BillingLine>(data.Bill);
                Bill.CollectionChanged += Bill_CollectionChanged;
            } }

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
        public ObservableCollection<BillingLine> Bill
        {
            get; protected set;
        }


        string newDesc;
        [JsonIgnore]
        public string Description
        {
            get
            {
                return Data.Description;
            }

            set
            {
                SetProperty<string>(ref newDesc, value, "Description");
                Data.Description = newDesc;
            }
        }

        private string title;
        [JsonIgnore]
        public string Title
        {
            get
            {
                return Data.Title;
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
                OnPlatform<Font> lfs = (OnPlatform<Font>)App.Current.Resources["LargeFontSize"];
                OnPlatform<Color> etc = (OnPlatform<Color>)App.Current.Resources["EmphasisTextColor"];

                return new FormattedString
                {

                    Spans = {
                        new Span { Text = "Total TTC: " },
                        new Span { Text = Data.Total.ToString(),
                            ForegroundColor = etc.Android  ,
                            FontSize = (double) lfs.Android.FontSize },
                        new Span { Text = "€", FontSize = (double) lfs.Android.FontSize }
                    }
                };
            }
        }
    }
}
