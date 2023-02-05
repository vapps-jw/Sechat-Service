using System.Linq;

namespace Sechat.Service.Dtos.ChatDtos;

public class UserConnectionDto
{
    public long Id { get; set; }
    public bool Approved { get; set; }
    public string InviterName { get; set; } = string.Empty;
    public string InvitedName { get; set; } = string.Empty;

    public bool Blocked { get; set; }
    public string BlockedByName { get; set; } = string.Empty;

    public bool UserPresent(string userName) => InviterName.Equals(userName) || InvitedName.Equals(userName);
    public bool UserPresents(string[] userNames) => userNames.All(un => InviterName.Equals(un) || InvitedName.Equals(un));
}