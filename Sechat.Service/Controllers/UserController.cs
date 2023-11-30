using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sechat.Data;
using Sechat.Data.Repositories;
using Sechat.Service.Configuration;
using Sechat.Service.Dtos;
using Sechat.Service.Dtos.ChatDtos;
using Sechat.Service.Dtos.CryptoDtos;
using Sechat.Service.Hubs;
using Sechat.Service.Services;
using Sechat.Service.Services.CacheServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sechat.Service.Controllers;

[Authorize]
[Route("[controller]")]
[ResponseCache(CacheProfileName = AppConstants.CacheProfiles.NoStore)]
public class UserController : SechatControllerBase
{
    private readonly ContactSuggestionsService _contactSuggestionsService;
    private readonly SignalRCache _cacheService;
    private readonly IDbContextFactory<SechatContext> _contextFactory;
    private readonly CryptographyService _cryptographyService;
    private readonly PushNotificationService _pushNotificationService;
    private readonly IMapper _mapper;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
    private readonly UserRepository _userRepository;

    public UserController(
        ContactSuggestionsService contactSuggestionsService,
        SignalRCache signalRConnectionsMonitor,
        IDbContextFactory<SechatContext> contextFactory,
        CryptographyService cryptographyService,
        PushNotificationService pushNotificationService,
        IMapper mapper,
        UserManager<IdentityUser> userManager,
        IHubContext<ChatHub, IChatHub> chatHubContext,
        UserRepository userRepository)
    {
        _contactSuggestionsService = contactSuggestionsService;
        _cacheService = signalRConnectionsMonitor;
        _contextFactory = contextFactory;
        _cryptographyService = cryptographyService;
        _pushNotificationService = pushNotificationService;
        _mapper = mapper;
        _userManager = userManager;
        _chatHubContext = chatHubContext;
        _userRepository = userRepository;
    }

    [HttpPost("suggest-contacts")]
    public async Task<IActionResult> GetSuggestedContacts([FromBody] UserControllerForms.SuggestedContacts suggestedContacts, CancellationToken cancellationToken)
    {
        var suggestions = await _contactSuggestionsService.CreateSuggectionsList(UserName, suggestedContacts.Data, cancellationToken);
        return Ok(suggestions);
    }

    [HttpGet("get-profile")]
    public async Task<IActionResult> GetProfile([FromServices] UserDataService userDataService)
    {
        var profile = await userDataService.GetProfile(UserId, UserName);
        return Ok(profile);
    }

    [HttpPost("request-contact")]
    public async Task<IActionResult> Invite([FromBody] ConnectionRequestDto invitationDto, CancellationToken cancellationToken)
    {
        if (UserName.Equals(invitationDto.Username)) return BadRequest("You cant invite yourself");

        var invitedUser = await _userManager.FindByNameAsync(invitationDto.Username);
        if (invitedUser is null) return BadRequest(AppConstants.ApiResponseMessage.DefaultFail);

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
            var contactDto = _mapper.Map<ContactDto>(newContact);
            contactDto.ProfileImage = _userRepository.GetProfilePicture(invitedUser.Id);

            contactDto.ProfileImage = _userRepository.GetProfilePicture(UserId);
            await _chatHubContext.Clients.Group(invitedUser.Id).ContactRequestReceived(contactDto);

            contactDto.ProfileImage = _userRepository.GetProfilePicture(invitedUser.Id);
            await _chatHubContext.Clients.Group(UserId).ContactRequestReceived(contactDto);

            await _pushNotificationService.IncomingContactRequestNotification(invitedUser.Id, UserName);
            await _contactSuggestionsService.UpdateCache(UserName, cancellationToken);
            return Ok("Invitation sent");
        }

        throw new Exception("Error when creating connection request");
    }

    [HttpDelete("delete-contact")]
    public async Task<IActionResult> DeleteContact(long contactId, CancellationToken cancellationToken)
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
            await _contactSuggestionsService.DeleteContact(UserName, cancellationToken);
            return Ok();
        }

        return BadRequest("Something went wrong");
    }

    [HttpPatch("block-contact")]
    public async Task<IActionResult> BlockContact(long contactId, CancellationToken cancellationToken)
    {
        var contact = _userRepository.BlockContact(contactId, UserId, UserName);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            await _chatHubContext.Clients.Group(await GetUserId(contactDto.InvitedName)).ContactUpdated(contactDto);
            await _chatHubContext.Clients.Group(await GetUserId(contactDto.InviterName)).ContactUpdated(contactDto);
            await _contactSuggestionsService.UpdateCache(UserName, cancellationToken);
            return Ok();
        }

        return BadRequest("Cant do that");
    }

    [HttpPatch("allow-contact")]
    public async Task<IActionResult> AllowContact(long contactId, CancellationToken cancellationToken)
    {
        var contact = _userRepository.AllowContact(contactId, UserId);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var contactDto = _mapper.Map<ContactDto>(contact);
            var pictures = await _userRepository.GetProfilePictures(new List<string> { contact.InviterId, contact.InvitedId }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            contactDto.ContactState = _cacheService.IsUserOnline(contact.InvitedId);
            contactDto.ProfileImage = pictures[contact.InviterId];
            await _chatHubContext.Clients.Group(contact.InvitedId).ContactUpdated(contactDto);

            contactDto.ContactState = _cacheService.IsUserOnline(contact.InviterId);
            contactDto.ProfileImage = pictures[contact.InvitedId];
            await _chatHubContext.Clients.Group(contact.InviterId).ContactUpdated(contactDto);
            await _contactSuggestionsService.UpdateCache(UserName, cancellationToken);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("invitations-permission")]
    public async Task<IActionResult> AllowInvitations([FromBody] UserControllerForms.FlagForm flagForm)
    {
        var profile = _userRepository.GetUserProfile(UserId);
        if (profile is null) return BadRequest("Profile does not exit");
        profile.InvitationsAllowed = flagForm.Flag;

        return await _userRepository.SaveChanges() > 0 ? Ok() : BadRequest(AppConstants.ApiResponseMessage.DefaultFail);
    }

    [HttpPatch("approve-contact")]
    public async Task<IActionResult> ApproveContact(
        [FromServices] Channel<DefaultNotificationDto> channel,
        long contactId,
        CancellationToken cancellationToken)
    {
        var contact = _userRepository.ApproveContact(contactId, UserId);
        if (contact is null) return BadRequest("Can`t do that");

        if (await _userRepository.SaveChanges() > 0)
        {
            var pictures = await _userRepository.GetProfilePictures(new List<string> { contact.InviterId, contact.InvitedId }, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var contactDto = _mapper.Map<ContactDto>(contact);
            var inviterId = await GetUserId(contactDto.InviterName);

            contactDto.ContactState = AppConstants.ContactState.Online;

            contactDto.ProfileImage = pictures[inviterId];
            await _chatHubContext.Clients.Group(UserId).ContactUpdated(contactDto);

            contactDto.ProfileImage = pictures[UserId];
            await _chatHubContext.Clients.Group(inviterId).ContactUpdated(contactDto);

            await _chatHubContext.Clients.Group(inviterId).DMKeyRequested(new DMKeyRequest(UserName, contactDto.InviterName, contactDto.Id));
            await channel.Writer.WriteAsync(new DefaultNotificationDto(AppConstants.PushNotificationType.ContactRequestApproved, inviterId, UserName));
            await _contactSuggestionsService.UpdateCache(UserName, cancellationToken);
            return Ok();
        }

        return BadRequest("Can`t do that");
    }

    [HttpPatch("profile-picture")]
    public async Task<IActionResult> PrepareProfilePicture(IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null) return BadRequest("Image not detected");

        using var ctx = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var user = await ctx.UserProfiles.FirstOrDefaultAsync(x => x.Id.Equals(UserId), cancellationToken);

        await using var stream = new MemoryStream();
        using var imageProcessor = await Image.LoadAsync(image.OpenReadStream(), cancellationToken);

        imageProcessor.Mutate(x => x.Resize(48, 48, KnownResamplers.Lanczos3));
        await imageProcessor.SaveAsync(stream, new PngEncoder(), cancellationToken);
        var imageData = Convert.ToBase64String(stream.ToArray());

        user.ProfilePicture = imageData;
        if (await ctx.SaveChangesAsync(cancellationToken) > 0)
        {
            await _contactSuggestionsService.DeleteContact(UserName, cancellationToken);
            return Ok(new UserControllerForms.ProcessedImageResponse(imageData));
        }

        return BadRequest(AppConstants.ApiResponseMessage.DefaultFail);
    }

    private async Task<string> GetUserId(string userName) => (await _userManager.FindByNameAsync(userName))?.Id;

}

public class UserControllerForms
{
    public record ProcessedImageResponse(string Data);

    public class ProfileImageForm
    {
        public string Data { get; set; }
    }
    public class ProfileImageFormValidation : AbstractValidator<ProfileImageForm>
    {
        public ProfileImageFormValidation() => _ = RuleFor(x => x.Data).NotNull().NotEmpty();
    }

    public class SuggestedContacts
    {
        public List<string> Data { get; set; } = new();
    }

    public class FlagForm
    {
        public bool Flag { get; set; }
    }
    public class FlagFormValidation : AbstractValidator<FlagForm>
    {
        public FlagFormValidation() => _ = RuleFor(x => x.Flag).NotNull();
    }
}
