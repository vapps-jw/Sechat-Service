namespace Sechat.Data.Models;
public record Token : BaseModel<long>
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public long UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
}
