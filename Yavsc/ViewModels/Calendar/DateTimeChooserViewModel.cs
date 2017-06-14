using System;
using Yavsc.Models.Calendar;


namespace Yavsc.ViewModels.Calendar
{
    public class DateTimeChooserViewModel
    {
        public string InputId { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public Period [] DisabledTimeIntervals { get; set; }
    }
}
