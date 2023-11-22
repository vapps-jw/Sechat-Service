using System;

namespace Sechat.Service.Dtos;

public class UserProfileProjection
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string ProfilePicture { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool InvitationsAllowed { get; set; }
    public string ReferallPass { get; set; }

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
