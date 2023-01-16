using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models;

public record Feature : BaseModel<long>
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    public List<UserProfile> UserProfiles { get; set; } = new();
}
