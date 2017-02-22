using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZicMoove.Model.UI
{
    public enum EventSeverity
    {
        Info,
        Warning,
        Critical
    }
    /// <summary>
    /// Encapsule un événement concernant le fonctionnement
    /// de cette présente application.
    /// </summary>
    class AppEvent
    {
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string Component { get; set; }
        public EventSeverity Severity { get; set; }
    }
}
