using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sechat.Service.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class AuthForecastController : ControllerBase
{
    private readonly ILogger<AuthForecastController> _logger;

    public AuthForecastController(ILogger<AuthForecastController> logger) => _logger = logger;

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login() => Ok();

    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register() => Ok();

    [HttpPost("logout")]
    public IActionResult Logout() => Ok();
}
