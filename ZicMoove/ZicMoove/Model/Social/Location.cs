

using System;
using Android.Runtime;
using ZicMoove.Model.Workflow.Messaging;

namespace ZicMoove.Model.Social
{

    /// <summary>
    /// Position.
    /// </summary>
    public class Position
    {
        public Position()
        {
        }

        public Position(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

        /// <summary>
        /// The longitude.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        ///
        /// The latitude.
        /// </summary>
        public double Latitude { get; set; }

    }


    public class Location : Position
    {

        public long Id { get; set; }
        public string Address { get; set; }

    }

    public class LocalizedEvent : YaEvent
    {
        public Location Location { get; set; }
    }
}