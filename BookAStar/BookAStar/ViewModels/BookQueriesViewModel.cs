using BookAStar.Helpers;
using BookAStar.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookAStar.ViewModels
{
    class BookQueriesViewModel : XLabs.Forms.Mvvm.ViewModel
    {
        public ObservableCollection<BookQueryData> Queries
        {
            get
            {
                return DataManager.Current.BookQueries;
            }
        }
    }
}
