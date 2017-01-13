using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BookAStar.ViewModels.EstimateAndBilling
{
    using Data;
    using Model;
    using System.Linq;

    public class BookQueriesViewModel : XLabs.Forms.Mvvm.ViewModel
    {
        public BookQueriesViewModel()
        {
            queries = new ObservableCollection<BookQueryViewModel>
                (DataManager.Instance.BookQueries.Select(
                    q =>
                    new BookQueryViewModel(q)));
        }
        private ObservableCollection<BookQueryViewModel> queries;

        public ObservableCollection<BookQueryViewModel> Queries
        {
            get
            {
                return queries;
            }
        }

        public ICommand RefreshQueries
        {
            get; set;
        }
    }
}
