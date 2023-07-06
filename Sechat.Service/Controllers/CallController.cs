using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos.ChatDtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class CallController : SechatControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly UserRepository _userRepository;
    private readonly ChatRepository _chatRepository;
    private readonly IMapper _mapper;

    public CallController(
        UserManager<IdentityUser> userManager,
        UserRepository userRepository,
        ChatRepository chatRepository,
        IMapper mapper)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _mapper = mapper;
    }

    [HttpGet("logs/{lastLog:long?}")]
    public IActionResult GetCallLogs(long lastLog = 0)
    {
        if (lastLog != 0)
        {
            var updates = _chatRepository.GetLogUpdates(UserId, lastLog);
            var updatesProfiles = updates.Select(u => u.UserProfile).ToDictionary(k => k.UserName, v => v);
            var dtos = _mapper.Map<List<CallLogDto>>(updates);
            foreach (var dto in dtos)
            {
                if (dto.CalleeName.Equals(UserName))
                {
                    dto.PhonerName = updatesProfiles[dto.UserProfileId].UserName;
                    continue;
                }

                dto.PhonerName = UserName;
            }
            return Ok(dtos);
        }

        var logs = _chatRepository.GetAllLogs(UserId);
        var profiles = logs.Select(u => u.UserProfile).ToDictionary(k => k.UserName, v => v);
        var allDtos = _mapper.Map<List<CallLogDto>>(logs);
        foreach (var dto in allDtos)
        {
            if (dto.CalleeName.Equals(UserName))
            {
                dto.PhonerName = profiles[dto.UserProfileId].UserName;
                continue;
            }

            dto.PhonerName = UserName;
        }

        return Ok(allDtos);
    }

    [HttpPost("log")]
    public async Task<IActionResult> RegisterCall([FromBody] CallControllerForms.CaleeNameForm form)
    {
        if (_userRepository.CheckContactAndGetContactId(UserName, form.CaleeName, out var contactId))
        {
            var res = _chatRepository.CreateNewCallLog(contactId, UserId);
            if (await _chatRepository.SaveChanges() > 0)
            {
                var dto = _mapper.Map<CallLogDto>(res);
                dto.PhonerName = UserName;
                return Ok(dto);
            }
        }

        return BadRequest();
    }

    [HttpPatch("logs-viewed")]
    public async Task<IActionResult> LogsViewed()
    {
        _chatRepository.MarkCallLogsAsViewed(UserId);
        _ = await _chatRepository.SaveChanges();
        return Ok();
    }

    [HttpPatch("answer")]
    public async Task<IActionResult> CallAnswered([FromBody] CallControllerForms.CaleeNameForm form)
    {
        if (_userRepository.CheckContactAndGetContactId(UserName, form.CaleeName, out var contactId))
        {
            _chatRepository.CallAnswered(contactId, UserId);
            if (await _chatRepository.SaveChanges() > 0)
            {
                return Ok();
            }
        }

        return BadRequest();
    }

    [HttpPatch("reject")]
    public async Task<IActionResult> CallRejected([FromBody] CallControllerForms.CaleeNameForm form)
    {
        if (_userRepository.CheckContactAndGetContactId(UserName, form.CaleeName, out var contactId))
        {
            _chatRepository.CallRejected(contactId, UserId);
            if (await _chatRepository.SaveChanges() > 0)
            {
                return Ok();
            }
        }

        return BadRequest();
    }
}

public class CallControllerForms
{
    public class CaleeNameForm
    {
        public string CaleeName { get; set; }
    }

    public class CaleeNameFormValidation : AbstractValidator<CaleeNameForm>
    {
        public CaleeNameFormValidation() => _ = RuleFor(x => x.CaleeName).NotEmpty().MaximumLength(AppConstants.StringLengths.UserNameMax);
    }
}
