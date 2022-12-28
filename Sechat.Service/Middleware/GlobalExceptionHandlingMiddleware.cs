using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Service.Config;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sechat.Service.Middleware
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger) => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var problem = new ProblemDetails()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Type = "Server error",
                    Title = "Server error",
                    Detail = "Server error occured - please contact Admin",
                };

                var json = JsonSerializer.Serialize(problem);
                context.Response.ContentType = AppConstants.ContentTypes.Json;
                await context.Response.WriteAsync(json);
            }
        }
    }
}
