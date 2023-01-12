using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;

namespace Sechat.Service.Hubs;

public abstract class SechatHubBase<T> : Hub<IChatHub>
{
    protected string UserId => GetClaim(ClaimTypes.NameIdentifier);
    protected string UserName => GetClaim(ClaimTypes.Name);
    protected string UserEmail => GetClaim(ClaimTypes.Email);

    private string GetClaim(string claimType) => Context.User.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value;
}
