using System;
using System.Collections.Generic;

namespace Heartbeat.Models
{
    public class AppSettings
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthEndpoint { get; set; }
        public string TokenAudience { get; set; }

        public DateTime StartCountAvailabilityFromDate { get; set; }
        public int AvgAvailabilityForDays { get; set; }
        public int AvgLatencyPerMinutes { get; set; }
        public double MinAvailability { get; set; }
        public int MaxLatency { get; set; }
        public IList<AzureApplication> Applications { get; set; }
        public IList<string> Locations { get; set; }
        public string Environment { get; set; }
    }
}
