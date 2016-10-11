using System.Collections.Generic;
using XLabs.Forms.Mvvm;
using BookAStar.Model.Workflow;
using System.Collections.ObjectModel;
using BookAStar.Model;
using Xamarin.Forms;

namespace BookAStar.ViewModels
{
    public class EditEstimateViewModel : ViewModel
    {
        public EditEstimateViewModel(Estimate data)
        {
            Data = data;
            if (data.AttachedFiles == null) data.AttachedFiles = new List<string>();
            if (data.AttachedGraphics == null) data.AttachedGraphics = new List<string>();
            if (data.Bill == null) data.Bill = new List<BillingLine>();
            AttachedFiles = new ObservableCollection<string>(data.AttachedFiles);
            AttachedGraphicList = new ObservableCollection<string>(data.AttachedGraphics);
            Bill = new ObservableCollection<BillingLine>(data.Bill);
        }

        public Estimate Data { get; protected set; }

        public ObservableCollection<string> AttachedFiles
        {
            get; protected set;
        }

        public ObservableCollection<string> AttachedGraphicList
        {
            get; protected set;
        }

        public ObservableCollection<BillingLine> Bill
        {
            get; protected set;
        }


        string newDesc;
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

        private int? status;
        public int? Status
        {
            get
            {
                return Data.Status;
            }

            set
            {
                SetProperty<int?>(ref status, value, "Status");
                Data.Status = status;
            }
        }
        private string title;
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

        public ClientProviderInfo Client {  get { return Data.Client; } }

        public BookQueryData Query { get { return Data.Query; } }

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
