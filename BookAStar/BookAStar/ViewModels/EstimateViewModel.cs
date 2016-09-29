using BookAStar.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Forms.Mvvm;
using BookAStar.Model.Workflow;
using System.Collections.ObjectModel;
using BookAStar.Model;

namespace BookAStar.ViewModels
{
    class EstimateViewModel : ViewModel
    {
        public EstimateViewModel(Estimate data)
        {
            estimate = data;
            AttachedFiles = new ObservableCollection<string>(data.AttachedFiles);
            AttachedGraphicList = new ObservableCollection<string>(data.AttachedGraphicList);
            Bill = new ObservableCollection<BillingLine>(data.Bill);
            description = data.Description;
            title = data.Title;
            status = data.Status;
        }

        Estimate estimate;
        
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


        private string description;
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                SetProperty<string>(ref description, value, "Description");
            }
        }

        private int? status;
        public int? Status
        {
            get
            {
                return status;
            }

            set
            {
                SetProperty<int?>(ref status, value, "Status");
            }
        }
        private string title;
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                SetProperty<string>(ref title, value, "Title");
            }
        }

        public ClientProviderInfo Client {  get { return estimate.Client; } }

        public BookQueryData Query { get { return estimate.Query; } }

    }
}
