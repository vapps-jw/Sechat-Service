namespace Sechat.Data.Models;
public record Key : BaseModel<long>
{
    public KeyType Type { get; set; }
    public string Value { get; set; } = string.Empty;

    public string UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
}
