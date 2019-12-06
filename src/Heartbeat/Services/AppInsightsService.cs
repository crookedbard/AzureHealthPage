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

            var availabilityResult = await client.QueryWithHttpMessagesAsync("availabilityResults | summarize Succeeded = count(success == 1), Failed = count(success == 0) by appName | extend Availability = 100.0 - Failed * 100.0 / Succeeded ",
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
