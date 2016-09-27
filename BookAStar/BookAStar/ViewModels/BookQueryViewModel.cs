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
    class BookQueryViewModel : XLabs.Forms.Mvvm.ViewModel
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
        }
        public ClientProviderInfo Client { get; set; }
        public Location Location { get; set; }
        public long Id { get; set; }
        public DateTime EventDate { get; set; }
        public decimal? Previsionnal { get; set; }
    }
}
