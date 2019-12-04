using Microsoft.AspNetCore.Builder;

namespace Heartbeat.Extensions
{
    public static class RequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestMiddleware(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<RequestMiddleware>();
            return builder;
        }
    }
}
