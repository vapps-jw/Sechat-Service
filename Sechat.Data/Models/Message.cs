namespace Sechat.Data.Models;

public record Message : BaseModel<long>
{
    public string Text { get; set; } = string.Empty;

    public long RoomId { get; set; }
    public Room Room { get; set; }
}
