using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.UserDetails;
public record Blacklisted : BaseModel<long>
{
    public string UserName { get; set; }

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
