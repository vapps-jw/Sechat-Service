using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sechat.Service.Dtos;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class AuthController : SechatControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger) => _logger = logger;

    [AllowAnonymous]
    [HttpPost("forgot-password-request")]
    public IActionResult ForgotPasswordRequest([FromBody] EmailForm emailForm) => Ok();

    [AllowAnonymous]
    [HttpPost("forgot-password-action")]
    public IActionResult ForgotPasswordAction([FromBody] PasswordResetForm passwordResetForm) => Ok();

    [HttpPost("change-password", Name = "ChangePassword")]
    public IActionResult ChangePassword([FromBody] PasswordChangeForm changePasswordForm) => Ok();
}
