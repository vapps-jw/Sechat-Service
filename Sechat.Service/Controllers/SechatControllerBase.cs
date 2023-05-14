using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace Sechat.Service.Controllers;

[ApiController]
public abstract class SechatControllerBase : ControllerBase
{
    protected string UserId => GetClaim(ClaimTypes.NameIdentifier);
    protected string UserName => GetClaim(ClaimTypes.Name);

    private string GetClaim(string claimType) => User.Claims.FirstOrDefault(x => x.Type.Equals(claimType))?.Value;
}
