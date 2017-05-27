
using System;
using System.Globalization;
using System.Windows.Input;
using System.ComponentModel;
using Yavsc.Billing;

namespace ZicMoove.ViewModels.EstimateAndBilling
{
    using Attributes;
    using Model.Workflow;
    using Validation;
    public class BillingLineViewModel : EditingViewModel<BillingLine>, ICommandLine
    {
        public ICommand RemoveCommand { get; set; }
        public ICommand ValidateCommand { set; get; }

        public BillingLineViewModel(BillingLine data): base(data)
        {
            SyncData();
        }

        private void SyncData()
        {
            if (Data != null)
            {
                // set durationValue, durationUnit
                Duration = Data.Duration;
                // other redondant representation
                count = Data.Count;
                description = Data.Description;
                unitaryCostText = Data.UnitaryCost.ToString("G", CultureInfo.InvariantCulture);
            }
            Check();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName=="Data")
            {
                SyncData();
            }
        }

        public override void Check()
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(Data.Description))
            {
                ModelState.AddError("Description", Strings.NoDescription);
            }
            if (Data.UnitaryCost < 0) { ModelState.AddError("UnitaryCost", Strings.InvalidValue); }
            if (Data.Count < 0) { ModelState.AddError("Count", Strings.InvalidValue); }
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
                Data.Count = count;
            }
        }
        private long estimateId;
        public long EstimateId
        {
            get
            {
                return estimateId;
            }
            set
            {
                SetProperty<long>(ref estimateId, value);
                Data.EstimateId = estimateId;
            }
        }

        private long id;
        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                SetProperty<long>(ref id, value);
                Data.Id = id;
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
                Data.Description = description;
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
                Data.UnitaryCost = unitaryCost;
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
                Data.Duration = this.Duration;
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
                Data.Duration = this.Duration;
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
