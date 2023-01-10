using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : SechatControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger) => _logger = logger;

    [HttpGet("confirm-email")]
    public IActionResult ConfirmEmail(string token) => Ok();

    [HttpDelete("delete-email")]
    public IActionResult DeleteEmail() => Ok();

    [HttpPost("update-email")]
    public IActionResult UpdateEmail() => Ok();
}
