using System;
using Yavsc.Models.Calendar;
using Yavsc.Server.Models.Calendar;

namespace Yavsc.ViewModels.Calendar
{
    public class DateTimeChooserViewModel
    {
        public string InputId { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public Period [] Busy { get; set; }
        public Period [] Free { get; set; }

        public string [] FreeDates { get ; set; }
        public string [] BusyDates { get ; set; }

    }
}
