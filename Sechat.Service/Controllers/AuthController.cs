using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger) => _logger = logger;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromServices] SignInManager<IdentityUser> signInManager)
    {
        var res = await signInManager.PasswordSignInAsync("u1", "u1", true, false);
        return Ok(res.Succeeded);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public IActionResult Register() => Ok();

    [HttpPost("logout")]
    public IActionResult Logout() => Ok();

    [HttpGet("test")]
    public IActionResult Test() => Ok("SECRET");
}
