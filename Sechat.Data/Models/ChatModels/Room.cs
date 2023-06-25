using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.ChatModels;

public record Room : BaseTrackedModel<string>
{
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public byte[] RoomKey { get; set; }
    public bool EncryptedByUser { get; set; }

    public List<Message> Messages { get; set; } = new();
    public List<UserProfile> Members { get; set; } = new();
}
