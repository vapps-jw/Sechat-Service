using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models;

public record Room : BaseTrackedModel<string>
{
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public string RoomKey { get; set; } = string.Empty;
    public bool EncryptedByUser { get; set; }

    public List<Message> Messages { get; set; } = new();
    public List<UserProfile> Members { get; set; } = new();
}
