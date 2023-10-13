using Sechat.Service.Configuration;
using Sechat.Service.Dtos.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Sechat.Service.Dtos.ChatDtos;

public class ContactDto
{
    public long Id { get; set; }
    public bool Approved { get; set; }
    public string ContactState { get; set; } = AppConstants.ContactState.Unknown;
    public string InviterName { get; set; } = string.Empty;
    public string InvitedName { get; set; } = string.Empty;
    public string ProfileImage { get; set; } = string.Empty;
    public bool Blocked { get; set; }
    public string BlockedByName { get; set; } = string.Empty;
    public bool Verified { get; set; }
    public List<DirectMessageDto> DirectMessages { get; set; } = new();
    public bool UserPresent(string userName) => InviterName.Equals(userName) || InvitedName.Equals(userName);
    public bool UserPresents(string[] userNames) => userNames.All(un => InviterName.Equals(un) || InvitedName.Equals(un));
}

public record BlacklistedDto(string UserName, string UserProfileId);

