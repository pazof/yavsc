using BookAStar.Interfaces;
using BookAStar.Model.Workflow;
using System;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    public class BillingLineViewModel : ViewModel, IBillingLine
    {
        BillingLine data;

        public BillingLineViewModel( BillingLine data)
        {
            this.data = (data == null) ? new BillingLine() : data;
            // sets durationValue & unit
            Duration = data.Duration;
        }

        protected int count;
        public int Count
        {
            get
            {
                return data.Count;
            }

            set
            {
                SetProperty<int>(ref count, value, "Count");
                data.Count = count;
            }
        }
        protected string description;
        public string Description
        {
            get
            {
                return data.Description;
            }

            set
            {
                SetProperty<string>(ref description, value, "Description");
                data.Description = value;
            }
        }
        protected int durationValue;
        public int DurationValue
        {
            get
            {
                return durationValue;
            }

            set
            {
                SetProperty<int>(ref durationValue, value, "DurationValue");
                data.Duration = this.Duration;
            }
        }

        public enum DurationUnits:int
        {
            Jours=0,
            Heures=1,
            Minutes=2
        }
        private DurationUnits durationUnit;
        public DurationUnits DurationUnit
        {
            get {
                return durationUnit;
            }
            set
            {
                SetProperty<DurationUnits>(ref durationUnit, value, "DurationUnit");
                data.Duration = this.Duration;
            }
        }

        protected decimal unitaryCost;
        public static readonly string unitCostFormat = "0,.00";
        public string UnitaryCostText
        {
            get
            {
                return unitaryCost.ToString(unitCostFormat, CultureInfo.InvariantCulture);
            }

            set
            {
                decimal newValue;
                if (decimal.TryParse(value, NumberStyles.Currency,
                    CultureInfo.InvariantCulture,
                    out newValue))
                {
                    SetProperty<decimal>(ref unitaryCost, newValue, "UnitaryCostText");
                    SetProperty<bool>(ref invalidCost, false, "InvalidCost");
                }
                else
                    SetProperty<bool>(ref invalidCost, true, "InvalidCost");
            }
        }
        bool invalidCost;
        public bool InvalidCost
        {
            get { return invalidCost; } 
        }
        public ICommand ValidateCommand { set; get; }

        public TimeSpan Duration
        {
            get
            {
                switch (DurationUnit)
                {
                    case DurationUnits.Heures:
                        return new TimeSpan(DurationValue, 0, 0);
                    case DurationUnits.Jours:
                        return new TimeSpan(DurationValue*24, 0, 0);
                    case DurationUnits.Minutes:
                        return new TimeSpan(0, DurationValue, 0);
                    // Assert(false); since all units are treated bellow
                    default:
                        return new TimeSpan(0, 0, DurationValue);
                }
            }

            set
            {
                double days = value.TotalDays;
                if (days >= 1.0)
                {
                    DurationValue = (int) days;
                    DurationUnit = DurationUnits.Jours;
                    return;
                }
                double hours = value.TotalHours;
                if (hours >= 1.0)
                {
                    DurationValue = (int) hours;
                    DurationUnit = DurationUnits.Jours;
                    return;
                }
                DurationValue = (int) value.TotalMinutes;
                DurationUnit = DurationUnits.Minutes;
            }
        }

        public decimal UnitaryCost
        {
            get
            {
                return decimal.Parse(this.UnitaryCostText,CultureInfo.InvariantCulture);
            }

            set
            {
                UnitaryCostText = value.ToString(unitCostFormat, CultureInfo.InvariantCulture);
            }
        }
    }
}
