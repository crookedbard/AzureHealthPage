using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heartbeat.Helpers;
using Heartbeat.Models;
using Microsoft.Azure.ApplicationInsights;
using Microsoft.Rest.Azure.Authentication;

namespace Heartbeat.Services
{
    public interface IAppInsightsService
    {
        /// <summary>
        /// Uses Application Insights API Cross-Resource queries to get statistics data
        /// </summary>
        /// <returns></returns>
        Task<IList<ApplicationStatistic>> GetApplicationStatisticsAsync();
    }
    public class AppInsightsService : IAppInsightsService
    {
        private readonly AppSettings _appSettings;

        public AppInsightsService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public async Task<IList<ApplicationStatistic>> GetApplicationStatisticsAsync()
        {
            var result = new List<ApplicationStatistic>();

            var adSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(_appSettings.AuthEndpoint),
                TokenAudience = new Uri(_appSettings.TokenAudience),
                ValidateAuthority = true
            };

            // Authenticate with client secret (app key)
            var credentials = await ApplicationTokenProvider.LoginSilentAsync(
                _appSettings.Domain,
                _appSettings.ClientId,
                _appSettings.ClientSecret, 
                adSettings);

            // New up a client with credentials and AI application Id
            using var client = new ApplicationInsightsDataClient(credentials);
            var applicationList = _appSettings.Applications.Select(i => i.Resource).ToList();
                
            if(applicationList.Count == 0) throw new ApplicationException("There should be at least 1 application defined in appSettings.json");
            client.AppId = applicationList[0];

            var avgAvailabilityFromDateTime = DateTime.UtcNow.AddDays(-_appSettings.AvgAvailabilityForDays);
            if (avgAvailabilityFromDateTime < _appSettings.StartCountAvailabilityFromDate)
                avgAvailabilityFromDateTime = _appSettings.StartCountAvailabilityFromDate;

            // Avg SLA for last 30 days
            var availabilityResult = await client.QueryWithHttpMessagesAsync(
                "let start_time=startofday(datetime('"+ avgAvailabilityFromDateTime.ToString("yyyy-MM-dd") + "'));let end_time=now(); availabilityResults | where timestamp > start_time and timestamp < end_time | summarize heartbeat_per_minutes=countif(success == 1) by bin_at(timestamp, 5m, start_time),appName | extend available_per_minutes=iff(heartbeat_per_minutes>0, true, false) | summarize total_available_minutes=countif(available_per_minutes==true) by appName | extend total_number_of_buckets=round((end_time-start_time)/5m)+1 | extend availability_rate=total_available_minutes*100/total_number_of_buckets",
                TimeSpan.FromDays(_appSettings.AvgAvailabilityForDays),
                null,
                applicationList);

            var availabilityJson = await availabilityResult.Response.Content.ReadAsStringAsync();
            var availabilityContent = Newtonsoft.Json.JsonConvert.DeserializeObject<QueryResult>(availabilityJson);

            // Last latency and online status
            var latencyResult = await client.QueryWithHttpMessagesAsync(
                "availabilityResults | extend itemType = iif(itemType == 'availabilityResult',itemType,'') | summarize offline=countif(success==0), avg(duration) by appName,location",
                TimeSpan.FromMinutes(_appSettings.AvgLatencyPerMinutes),
                null,
                applicationList);

            var latencyJson = await latencyResult.Response.Content.ReadAsStringAsync();
            var latencyContent = Newtonsoft.Json.JsonConvert.DeserializeObject<QueryResult>(latencyJson);

            foreach (var app in _appSettings.Applications)
            {
                var stat = new ApplicationStatistic();
                stat.ApplicationName = app.Name;
                var availabilityRow = availabilityContent.Tables[0].Rows.SingleOrDefault(i => i[0].ToString() == app.Resource);
                if (availabilityRow == null) continue;
                stat.AvgAvailability = StringHelper.GetDouble(availabilityRow[3].ToString());

                var latencyRows = latencyContent.Tables[0].Rows.Where(i => i[0].ToString() == app.Resource);
                stat.LocationStatusAndLatencies = new List<ApplicationStatistic.LocationStatusAndLatency>();
                foreach (var row in latencyRows)
                {
                    var locationName = (string)row[1];
                    if (!_appSettings.Locations.Contains(locationName)) continue;
                    var lat = new ApplicationStatistic.LocationStatusAndLatency();
                    lat.IsOnline = StringHelper.GetInt(row[2].ToString()) == 0;
                    lat.LocationName = locationName;
                    lat.AvgLatency = StringHelper.GetInt(row[3].ToString());

                    stat.LocationStatusAndLatencies.Add(lat);
                }

                result.Add(stat);
            }

            return result;
        }
    }
}
