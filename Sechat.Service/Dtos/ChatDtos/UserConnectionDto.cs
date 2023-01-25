namespace Sechat.Service.Dtos.ChatDtos;

public class UserConnectionDto
{
    public long Id { get; set; }
    public bool Approved { get; set; }
    public string InviterId { get; set; } = string.Empty;
    public string InviterName { get; set; } = string.Empty;
    public string InvitedId { get; set; } = string.Empty;
    public string InvitedName { get; set; } = string.Empty;

    public bool Blocked { get; set; }
    public string BlockedByName { get; set; } = string.Empty;

    public bool UserPresent(string userId)
    {
        if (InviterId.Equals(userId) || InvitedId.Equals(userId))
        {
            return true;
        }
        return false;
    }
}