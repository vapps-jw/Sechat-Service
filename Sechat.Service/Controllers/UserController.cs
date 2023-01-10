using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sechat.Data.Repositories;
using Sechat.Service.Dtos;
using Sechat.Service.Services;
using Sechat.Service.Settings;
using System.Net;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : SechatControllerBase
{
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserRepository _userRepository;

    public UserController(
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        UserRepository userRepository)
    {
        _mapper = mapper;
        _userManager = userManager;
        _userRepository = userRepository;
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
