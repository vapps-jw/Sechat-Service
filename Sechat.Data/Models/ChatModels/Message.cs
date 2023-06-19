using Sechat.Data.Models.Abstractions;

namespace Sechat.Data.Models.ChatModels;

public record Message : BaseModel<long>
{
    public string IdSentBy { get; set; } = string.Empty;
    public string NameSentBy { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public string RoomId { get; set; }
    public Room Room { get; set; }

    public List<MessageViewer> MessageViewers { get; set; } = new();
}
