using BookAStar.Interfaces;
using BookAStar.Model.Workflow;
using System;
using System.Windows.Input;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    public class BillingLineViewModel : ViewModel, IBillingLine
    {
        BillingLine data;
        public Estimate Billing { protected set; get; }

        public BillingLineViewModel(Estimate billing, BillingLine data)
        {
            this.data = (data == null) ? new BillingLine() : data;
            Billing = billing;
            ValidateCommand = 
                new Command(
                () => {
                    Billing.Bill.Add(data);
                    Validated.Invoke(this, new EventArgs());
                });
        }

        protected int count;
        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                SetProperty<int>(ref count, value, "Count");
            }
        }
        protected string description;
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
        protected TimeSpan duration;
        public TimeSpan Duration
        {
            get
            {
                return duration;
            }

            set
            {
                SetProperty<TimeSpan>(ref duration, value, "Duration");
            }
        }

        protected decimal unitaryCost;
        public decimal UnitaryCost
        {
            get
            {
                return unitaryCost;
            }

            set
            {
                SetProperty<decimal>(ref unitaryCost, value, "UnitaryCost");
            }
        }

        public ICommand ValidateCommand { protected set; get; }

        public event EventHandler<EventArgs> Validated;

    }
}
