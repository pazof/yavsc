using BookAStar.Helpers;
using BookAStar.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BookAStar.ViewModels
{
    public class BookQueriesViewModel : XLabs.Forms.Mvvm.ViewModel
    {
        public BookQueriesViewModel()
        {

        }

        public ObservableCollection<BookQueryData> Queries
        {
            get
            {
                return DataManager.Current.BookQueries;
            }
        }

        public ICommand RefreshQueries
        {
            get { return DataManager.Current.BookQueries; }
        }
    }
}
