using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.ChatModels;
public record PrivateMessage : BaseModel<long>
{
    public string IdSentBy { get; set; } = string.Empty;
    public string NameSentBy { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
    public byte[] Key { get; set; }
    public bool EncryptedByUser { get; set; }

    public string UserProfileId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; }
}
