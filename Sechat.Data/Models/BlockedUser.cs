namespace Sechat.Data.Models;
public record BlockedUser : BaseModel<long>
{
    public string UserId { get; set; }

    public string UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
}
