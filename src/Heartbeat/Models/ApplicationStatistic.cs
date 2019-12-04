using System.Collections.Generic;

namespace Heartbeat.Models
{
    public class ApplicationStatistic
    {
        public string ApplicationName { get; set; }
        public double AvgAvailability { get; set; }
        public IList<LocationStatusAndLatency> LocationStatusAndLatencies { get; set; }
        public class LocationStatusAndLatency
        {
            public string LocationName { get; set; }
            public bool IsOnline { get; set; }
            public int AvgLatency { get; set; }
        }
    }
}
