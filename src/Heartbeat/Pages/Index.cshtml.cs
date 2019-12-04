using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Heartbeat.Models;
using Heartbeat.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAppInsightsService _appInsightsService;

        [BindProperty]
        public IList<ApplicationStatistic> ApplicationStatistics { get; set; }
        [BindProperty]
        public int AvgAvailabilityForDays { get; set; }
        [BindProperty]
        public int AvgLatencyPerMinutes { get; set; }
        [BindProperty]
        public int MaxLatency { get; set; }
        [BindProperty]
        public double MinAvailability { get; set; }
        [BindProperty]
        public IList<string> Locations { get; set; }
        [BindProperty]
        public string Environment { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IAppInsightsService appInsightsService, AppSettings appSettings)
        {
            _logger = logger;
            _appInsightsService = appInsightsService;

            AvgAvailabilityForDays = appSettings.AvgAvailabilityForDays;
            AvgLatencyPerMinutes = appSettings.AvgLatencyPerMinutes;
            MaxLatency = appSettings.MaxLatency;
            MinAvailability = appSettings.MinAvailability;
            Locations = appSettings.Locations;
            Environment = appSettings.Environment;
        }

        public async Task OnGet()
        {
            try
            {
                ApplicationStatistics = await _appInsightsService.GetApplicationStatisticsAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Index.cshtml.cs OnGet()");
                throw;
            }
        }
    }
}
