using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.Extensions.Logging;

namespace Heartbeat.Extensions
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestMiddleware> _logger;

        public RequestMiddleware(RequestDelegate next, ILogger<RequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BadHttpRequestException ex)
            {
                _logger.LogError(ex, "RequestMiddleware. HttpRequestWithStatusException.");
                context.Response.StatusCode = ex.StatusCode;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "RequestMiddleware. Exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}
