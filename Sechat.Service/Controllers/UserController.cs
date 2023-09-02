﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.CryptoDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
public class UserController : SechatControllerBase
{
    private readonly CryptographyService _cryptographyService;
    private readonly PushNotificationService _pushNotificationService;
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    private readonly UserRepository _userRepository;

    public UserController(
        CryptographyService cryptographyService,
        PushNotificationService pushNotificationService,
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        IHubContext<ChatHub, IChatHub> chatHubContext,
        UserRepository userRepository)
    {
        _cryptographyService = cryptographyService;
        _pushNotificationService = pushNotificationService;
        _mapper = mapper;
        _userManager = userManager;
        _chatHubContext = chatHubContext;
        _userRepository = userRepository;
    }

    [HttpGet("get-profile")]
    public async Task<IActionResult> GetProfile([FromServices] UserDataService userDataService)
    {
        var profile = await userDataService.GetProfile(UserId, UserName);
        return Ok(profile);
    }

    [HttpPost("request-contact")]
    public async Task<IActionResult> Invite([FromBody] ConnectionRequestDto invitationDto)
    {
        if (UserName.Equals(invitationDto.Username)) return BadRequest("You cant invite yourself");

        var invitedUser = await _userManager.FindByNameAsync(invitationDto.Username);
        if (invitedUser is null) return BadRequest("Something went wrong");

        var invitedProfile = _userRepository.GetUserProfile(invitedUser.Id);
        if (!invitedProfile.InvitationsAllowed)
        {
            return BadRequest("User dont want to be invited");
        }

        var contactExists = _userRepository.ContactExists(UserId, invitedUser.Id);
        if (contactExists) return BadRequest("Contact exists");

        var newKey = _cryptographyService.GenerateKey();
        var newContact = _userRepository.CreateContact(UserId, UserName, invitedUser.Id, invitedUser.UserName, newKey);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(invitedUser.Id).ContactRequestReceived(_mapper.Map<ContactDto>(newContact));
            await _chatHubContext.Clients.Group(UserId).ContactRequestReceived(_mapper.Map<ContactDto>(newContact));
            await _pushNotificationService.IncomingContactRequestNotification(invitedUser.Id, UserName);
            return Ok("Invitation sent");
        }

        throw new Exception("Error when creating connection request");
    }

    [HttpDelete("delete-contact")]
    public async Task<IActionResult> DeleteContact(long contactId)
    {
        var contact = await _userRepository.GetContact(contactId);
        if (contact is null) return BadRequest("Not your contact");

        if (contact.Blocked && !contact.BlockedById.Equals(UserId))
        {
            return BadRequest("You are blocked");
        }

        var contactDto = _mapper.Map<ContactDto>(contact);
        if (!contactDto.UserPresent(UserName))
        {
            return BadRequest("Not your contact");
        }

        _userRepository.DeleteContact(contactId);

        if (await _userRepository.SaveChanges() > 0)
        {
            await _chatHubContext.Clients.Group(contact.InvitedId).ContactDeleted(new ResourceId(contactId));
            await _chatHubContext.Clients.Group(contact.InviterId).ContactDeleted(new ResourceId(contactId));
            return Ok();
        }

        return BadRequest("Something went wrong");
    }

    [HttpPatch("block-contact")]
    public async Task<IActionResult> BlockContact(long contactId)
    {
        var contact = _userRepository.BlockContact(contactId, UserId, UserName);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            await _chatHubContext.Clients.Group(await GetUserId(contactDto.InvitedName)).ContactUpdated(contactDto);
            await _chatHubContext.Clients.Group(await GetUserId(contactDto.InviterName)).ContactUpdated(contactDto);
            return Ok();
        }

        return BadRequest("Cant do that");
    }

    [HttpPatch("allow-contact")]
    public async Task<IActionResult> AllowContact(long contactId)
    {
        var contact = _userRepository.AllowContact(contactId, UserId);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            await _chatHubContext.Clients.Group(contact.InvitedId).ContactUpdated(contactDto);
            await _chatHubContext.Clients.Group(contact.InviterId).ContactUpdated(contactDto);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("allow-invitations")]
    public async Task<IActionResult> AllowInvitations([FromBody] UserControllerForms.FlagForm flagForm)
    {
        var profile = _userRepository.GetUserProfile(UserId);
        if (profile is null) return BadRequest("Profile does not exit");
        profile.InvitationsAllowed = flagForm.Flag;

        return await _userRepository.SaveChanges() > 0 ? Ok() : BadRequest("Something went wrong");
    }

    [HttpPatch("approve-contact")]
    public async Task<IActionResult> ApproveContact(
        [FromServices] Channel<DefaultNotificationDto> channel,
        long contactId)
    {
        var contact = _userRepository.ApproveContact(contactId, UserId);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            var inviterId = await GetUserId(contactDto.InviterName);

            contactDto.ContactState = AppConstants.ContactState.Online;

            await _chatHubContext.Clients.Group(UserId).ContactUpdated(contactDto);
            await _chatHubContext.Clients.Group(inviterId).ContactUpdated(contactDto);
            await _chatHubContext.Clients.Group(inviterId).DMKeyRequested(new DMKeyRequest(UserName, contactDto.InviterName, contactDto.Id));
            await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.ContactRequestApproved, inviterId, UserName));
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    private async Task<string> GetUserId(string userName) => (await _userManager.FindByNameAsync(userName))?.Id;

}

public class UserControllerForms
{
    public class FlagForm
    {
        public bool Flag { get; set; }
    }
    public class FlagFormValidation : AbstractValidator<FlagForm>
    {
        public FlagFormValidation() => _ = RuleFor(x => x.Flag).NotNull().NotEmpty();
    }
}
