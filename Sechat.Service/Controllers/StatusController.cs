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
    [EnableRateLimiting(AppConstants.RateLimiting.MinimalRateLimiterPolicy)]
    public IActionResult PingApi() => Ok();

    [HttpGet("ping-api-global")]
    public IActionResult PingApiGlobal() => Ok();

}
