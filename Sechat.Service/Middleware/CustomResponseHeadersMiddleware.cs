using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Middleware;

public class CustomResponseHeadersMiddleware : IMiddleware
{
    private readonly ILogger<CustomResponseHeadersMiddleware> _logger;

    public CustomResponseHeadersMiddleware(ILogger<CustomResponseHeadersMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            context.Response.Headers.Add("API-RES", "sechat-server-response");
            context.Response.Headers.Add("X-Developed-By", "JWTK");

            context.Response.Headers[HeaderNames.CacheControl] = "max-age=0,no-cache,must-revalidate";
            context.Response.Headers[HeaderNames.Expires] = "Tue, 01 Jan 1970 00:00:00 GMT";
            context.Response.Headers[HeaderNames.Pragma] = "no-cache";

            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}
