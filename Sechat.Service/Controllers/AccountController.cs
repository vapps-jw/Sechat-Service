using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Services;
using Sechat.Service.Settings;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController : SechatControllerBase
{
    private readonly IMapper _mapper;
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserRepository _userRepository;

    public AccountController(
        IMapper mapper,
        ILogger<AccountController> logger,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        UserRepository userRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserCredentials userCredentials)
    {
        var signInResult = await _signInManager.PasswordSignInAsync(userCredentials.Username, userCredentials.Password, true, false);
        if (!signInResult.Succeeded)
        {
            return BadRequest();
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCredentials userCredentials)
    {
        var user = new IdentityUser(userCredentials.Username);
        var createUserResult = await _userManager.CreateAsync(user, userCredentials.Password);

        if (!createUserResult.Succeeded)
        {
            return BadRequest();
        }

        _logger.LogInformation($"User {userCredentials.Username} has been created");
        return Ok();
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager
            .FindByIdAsync(User.Claims
            .FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value);
        if (user is null)
        {
            return BadRequest("User not found");
        }

        _userRepository.DeleteUserProfile(UserId);

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return Problem();
        }

        _logger.LogInformation($"User {UserName} has been removed");
        return Ok();
    }

    [HttpGet("get-profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (!_userRepository.ProfileExists(UserId))
        {
            _userRepository.CreateUserProfile(UserId);
            if (await _userRepository.SaveChanges() == 0)
            {
                return Problem("Profile creation failed");
            }
        }

        return Ok(_mapper.Map<UserProfileProjection>(_userRepository.GetUserProfile(UserId)));
    }

    [HttpPut("update-email")]
    public async Task<IActionResult> UpdateEmail(
        IEmailClient emailClient,
        IOptionsMonitor<CorsSettings> corsSettings,
        [FromBody] EmailForm emailForm)
    {
        if (emailForm.Equals(UserEmail))
        {
            return BadRequest();
        }

        var currentUser = await _userManager.FindByIdAsync(UserId);
        var confirmationToken = await _userManager.GenerateChangeEmailTokenAsync(currentUser, emailForm.Email);

        var qb = new QueryBuilder
        {
            { "token", confirmationToken },
            { "email", emailForm.Email }
        };
        var callbackUrl = $@"{corsSettings.CurrentValue.ApiUrl}/account/confirm-email/{qb}";

        var sgResponse = await emailClient.SendEmailConfirmationAsync(emailForm.Email, callbackUrl);
        if (sgResponse.StatusCode != HttpStatusCode.Accepted)
        {
            return Problem();
        }

        return Ok();
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(string token, string email)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest();
        }

        var currentUser = await _userManager.FindByIdAsync(UserId);
        var confirmResult = await _userManager.ChangeEmailAsync(currentUser, email, token);

        if (!confirmResult.Succeeded)
        {
            return BadRequest();
        }

        return Ok();
    }
}
