using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.Middleware;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly Channel<DefaultNotificationDto> _pushNotificaionsChannel;

    public GlobalExceptionHandlingMiddleware(
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        Channel<DefaultNotificationDto> pushNotificaionsChannel)
    {
        _logger = logger;
        _pushNotificaionsChannel = pushNotificaionsChannel;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ChatException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "Chat error",
                Title = "Chat error",
                Detail = ex.Message,
            };

            var json = JsonSerializer.Serialize(problem);
            context.Response.ContentType = AppConstants.ContentTypes.Json;
            await context.Response.WriteAsync(json);
        }
        catch (OperationCanceledException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var problem = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "Request Cancelled",
                Title = "Request Cancelled",
                Detail = ex.Message,
            };

            var json = JsonSerializer.Serialize(problem);
            context.Response.ContentType = AppConstants.ContentTypes.Json;
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await _pushNotificaionsChannel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.ApplicationEvent, string.Empty, ex.Message));

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new ProblemDetails()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "Something went wrong",
                Title = "Something went wrong",
                Detail = "Something went wrong",
            };

            var json = JsonSerializer.Serialize(problem);
            context.Response.ContentType = AppConstants.ContentTypes.Json;
            await context.Response.WriteAsync(json);
        }
    }
}
