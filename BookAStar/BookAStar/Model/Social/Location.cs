

using System;
using Android.Runtime;
using BookAStar.Model.Workflow.Messaging;

namespace BookAStar.Model.Social
{

    /// <summary>
    /// Position.
    /// </summary>
    public class Position
    {
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


    public class Location : Position {
             
        public long Id { get; set; }
        public string Address { get; set; }

    }

    public class  LocalizedEvent : YaEvent
    {
       

        public Location Location { get; set; }

        }
}