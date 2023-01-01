namespace Sechat.Data.Models;

public record UserFeature
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public List<UserProfile> UserProfiles { get; set; } = new();
}
