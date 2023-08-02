using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Sechat.Service.Settings;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Hubs.Filters;

public class AuthException : HubException
{
    public AuthException(string message) : base(message)
    {
    }
}

public class AuthHubFilter : IHubFilter
{
    private readonly IOptionsMonitor<CookieSettings> _optionsMonitor;

    public AuthHubFilter(IOptionsMonitor<CookieSettings> optionsMonitor) => _optionsMonitor = optionsMonitor;

    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next) =>
        await next(invocationContext);

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next) => next(context);

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next) => next(context, exception);
}
