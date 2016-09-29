using BookAStar.Interfaces;
using BookAStar.Model.Workflow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    class BillingLineViewModel : ViewModel, IBillingLine
    {
        public BillingLineViewModel(BillingLine data)
        {
            if (data == null) data = new BillingLine();
            count = data.Count;
            description = data.Description;
            unitaryCost = data.UnitaryCost;
            duration = data.Duration;
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
    }
}
