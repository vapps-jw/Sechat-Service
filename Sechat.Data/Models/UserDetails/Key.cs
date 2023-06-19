using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.UserDetails;
public record Key : BaseModel<long>
{
    public KeyType Type { get; set; }
    public string Value { get; set; } = string.Empty;

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
