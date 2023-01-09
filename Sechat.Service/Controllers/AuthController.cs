using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Service.Dtos;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger) => _logger = logger;

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

    [HttpGet("confirm-email", Name = "ConfirmEmail")]
    public IActionResult ConfirmEmail(string token) => Ok();

    [HttpDelete("delete-account")]
    public IActionResult DeleteAccount() => Ok();

    [AllowAnonymous]
    [HttpPost("forgot-password-request")]
    public IActionResult ForgotPasswordRequest([FromBody] EmailForm emailForm) => Ok();

    [HttpPost("forgot-password-action")]
    public IActionResult ForgotPasswordAction([FromBody] PasswordResetForm passwordResetForm) => Ok();

    [HttpPost("change-password", Name = "ChangePassword")]
    public IActionResult ChangePassword([FromBody] UserChangePasswordForm changePasswordForm) => Ok();
}
