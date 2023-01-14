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

    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {
        var httpContext = invocationContext.Context.GetHttpContext();
        var authCookie = httpContext.Request.Cookies[_optionsMonitor.CurrentValue.AuthCookieName];

        Console.WriteLine("Cookies", authCookie);

        //// ONE - grab the CookieAuthenticationOptions instance
        //var opt = httpContext.RequestServices
        //    .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
        //    .Get("Identity.Application");

        //// TWO - Get the encrypted cookie value
        //var cookie = opt.CookieManager.GetRequestCookie(httpContext, opt.Cookie.Name);

        //// THREE - decrypt it
        //return opt.TicketDataFormat.Unprotect(cookie);

        //  var expiry = invocationContext.Context.User.Claims.FirstOrDefault(x => x.Type == "expires").Value;
        //var expiryDate = new DateTimeOffset(long.Parse(expiry), TimeSpan.Zero);
        //if (DateTimeOffset.UtcNow.Subtract(expiryDate) > TimeSpan.Zero)
        //{
        //    throw new AuthException("auth_expired");
        //    // await invocationContext.Hub.Clients.Caller.SendAsync("session_expired");
        //}

        return await next(invocationContext);
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next) => next(context);

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next) => next(context, exception);
}
