
using System;
using System.Diagnostics;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels.EstimateAndBilling
{
    using Data;
    using Interfaces;
    using Model;
    using Model.Social;
    using Model.Workflow;
    using System.Linq;

    class BookQueryViewModel : ViewModel, IBookQueryData
    {
        public BookQueryViewModel()
        {
            
        }
        public BookQueryViewModel(BookQueryData data)
        {
            Debug.Assert(data != null);
            Client=data.Client;
            Location = data.Location;
            EventDate = data.EventDate;
            Previsionnal = data.Previsionnal;
            Id = data.Id;
            this.data = data;
        }
        private BookQueryData data;
        public BookQueryData Data {
            get
            {
                return data;
            }
        }
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime EventDate { get; set; }
        public decimal? Previsionnal { get; set; }
        public EditEstimateViewModel DraftEstimate
        {
            get
            {
                return DataManager.Current.EstimationCache.LocalGet(this.Id);
            }
        }
        public Estimate Estimate { get
            {
                return DataManager.Current.Estimates.FirstOrDefault(
                    e=>e.Query.Id == Id
                    );
            } }


        public bool EstimationDone
        {
            get
            {
                return Estimate != null;
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
