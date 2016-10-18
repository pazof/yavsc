using BookAStar.Data;
using BookAStar.Interfaces;
using BookAStar.Model;
using BookAStar.Model.Social;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.ViewModels
{
    class BookQueryViewModel : XLabs.Forms.Mvvm.ViewModel, IBookQueryData
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
        public string EditEstimateButtonText
        {
            get
            {
                return DraftEstimate != null ? "Editer le devis" : "Faire un devis" ;
            }
        }
    }
}
