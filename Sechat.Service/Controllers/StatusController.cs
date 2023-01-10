using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sechat.Service.Controllers;

[Route("[controller]")]
public class StatusController : SechatControllerBase
{

    private readonly ILogger<StatusController> _logger;

    public StatusController(ILogger<StatusController> logger) => _logger = logger;

    [HttpGet("ping-api")]
    public IActionResult Test() => Ok();

}
