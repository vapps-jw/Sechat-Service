using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Sechat.Service.Utilities;

namespace Sechat.Service.Controllers;

[EnableRateLimiting(AppConstants.RateLimiting.DefaultWindowPolicyName)]
[Route("[controller]")]
public class StatusController : SechatControllerBase
{

    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger) => _logger = logger;

    [HttpGet("ping-api")]
    public IActionResult Test() => Ok();

}
