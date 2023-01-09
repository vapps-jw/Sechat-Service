using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger) => _logger = logger;

    [HttpGet("test")]
    public IActionResult Test() => Ok("SECRET");

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] SignInManager<IdentityUser> signInManager)
    {
        var res = await signInManager.PasswordSignInAsync("u1", "u1", true, false);
        return Ok(res.Succeeded);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register() => Ok();

    [HttpPost("logout")]
    public IActionResult Logout() => Ok();

    [HttpDelete("delete-account")]
    public IActionResult DeleteAccount() => Ok();
}
