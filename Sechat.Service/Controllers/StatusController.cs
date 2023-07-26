using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Sechat.Service.Configuration;

namespace Sechat.Service.Controllers;

[Route("[controller]")]
public class StatusController : SechatControllerBase
{

    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger) => _logger = logger;

    [HttpGet("ping-api")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public IActionResult PingApi() => Ok();

    [Authorize]
    [HttpGet("ping-authorized")]
    [EnableRateLimiting(AppConstants.RateLimiting.AnonymusRestricted)]
    public IActionResult PingAuthorized() => Ok("Authorized");
}
