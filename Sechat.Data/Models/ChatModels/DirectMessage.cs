using Sechat.Data.Models.Abstractions;
using Sechat.Data.Models.UserDetails;

namespace Sechat.Data.Models.ChatModels;
public record DirectMessage : BaseModel<long>
{
    public string FromId { get; set; }
    public string ToId { get; set; }

    public string Text { get; set; } = string.Empty;
    public bool WasViewed { get; set; }

    public long ContactId { get; set; }
    public Contact Contact { get; set; }
}
