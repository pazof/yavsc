﻿using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BookAStar.ViewModels
{
    using Data;
    using Model;
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
