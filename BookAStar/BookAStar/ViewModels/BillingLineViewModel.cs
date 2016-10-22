using BookAStar.Attributes;
using BookAStar.Interfaces;
using BookAStar.Model.Workflow;
using System;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;
using XLabs.Forms.Mvvm;

namespace BookAStar.ViewModels
{
    public class BillingLineViewModel : EditingViewModel, IBillingLine
    {
        BillingLine data;

        public BillingLineViewModel(BillingLine data)
        {
            this.data = data ?? new BillingLine();
            // sets durationValue & durationUnit
            count = data.Count;
            description = data.Description;

            Duration = data.Duration;
            unitaryCostText = data.UnitaryCost.ToString("G", CultureInfo.InvariantCulture);
        }

        private int count;
        public int Count
        {
            get
            {
                return count;
            }

            set
            {
                SetProperty<int>(ref count, value);
                data.Count = count;
            }
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
                SetProperty<string>(ref description, value);
                data.Description = description;
            }
        }
        decimal unitaryCost;
        public decimal UnitaryCost
        {
            get
            {
                return unitaryCost;
            }

            set
            {
                SetProperty<decimal>(ref unitaryCost, value);
                data.UnitaryCost = unitaryCost;
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

        public enum DurationUnits : int
        {
            Jours = 0,
            Heures = 1,
            Minutes = 2
        }
        private DurationUnits durationUnit;

        [Display(Name = "Unité de temps", Description = @"Unité de temps utiliée
pour décrire la quantité de travail associée à ce type de service")]
        public DurationUnits DurationUnit
        {
            get
            {
                return durationUnit;
            }
            set
            {
                SetProperty<DurationUnits>(ref durationUnit, value, "DurationUnit");
                data.Duration = this.Duration;
            }
        }
        
        public static readonly string unitCostFormat = "0,.00";
        string unitaryCostText;
        public string UnitaryCostText
        {
            get
            {
                return unitaryCostText;
            }

            set

            {
                SetProperty<string>(ref unitaryCostText, value, "UnitaryCostText");
                // TODO update behavior
                decimal test;
                if (decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out test))
                {
                    this.UnitaryCost = test;
                }
            }
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
                        return new TimeSpan(DurationValue * 24, 0, 0);
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
                    DurationValue = (int)days;
                    DurationUnit = DurationUnits.Jours;
                    return;
                }
                double hours = value.TotalHours;
                if (hours >= 1.0)
                {
                    DurationValue = (int)hours;
                    DurationUnit = DurationUnits.Jours;
                    return;
                }
                DurationValue = (int)value.TotalMinutes;
                DurationUnit = DurationUnits.Minutes;
            }
        }

    }
}
