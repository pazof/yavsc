
using System;
using System.Diagnostics;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.EstimateAndBilling
{
    using Data;
    using Helpers;
    using Interfaces;
    using Model;
    using Model.Social;
    using Model.Workflow;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Xamarin.Forms;

    public class BookQueryViewModel : ViewModel, IBookQueryData
    {
        public BookQueryViewModel()
        {
            
        }
        public BookQueryViewModel(BookQuery data)
        {
            Debug.Assert(data != null);
            Client=data.Client;
            Location = data.Location;
            EventDate = data.EventDate;
            Previsionnal = data.Previsionnal;
            Id = data.Id;
            estimates = new ObservableCollection<Estimate>(
                    DataManager.Instance.Estimates.Where(
                    e => e.Query.Id == Id
                    ));
            this.data = data;
        }
        private BookQuery data;
        public BookQuery Data {
            get
            {
                return data;
            }
        }
        public ClientProviderInfo Client { get; set; }
        public ImageSource Avatar
        {
            get
            {
                return UserHelpers.Avatar(Client.Avatar);
            }
        }
        public ImageSource SmallAvatar
        {
            get
            {
                return UserHelpers.SmallAvatar(Client.Avatar, Client.UserName);
            }
        }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime EventDate { get; set; }
        public decimal? Previsionnal { get; set; }
        public EditEstimateViewModel DraftEstimate
        {
            get
            {
                return DataManager.Instance.EstimationCache.LocalGet(this.Id);
            }
        }
        private ObservableCollection<Estimate> estimates;
        public ObservableCollection<Estimate> Estimates {
            get {
                return estimates;
            } }

        public bool EstimationDone
        {
            get
            {
                return Estimates != null && Estimates.Count>0;
            }
        }

        public string EditEstimateButtonText
        {
            get
            {
                return DraftEstimate != null ?
                        Strings.EditEstimate : Strings.DoEstimate;
            }
        }
    }
}
