using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}
