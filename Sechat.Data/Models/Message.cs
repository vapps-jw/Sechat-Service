namespace Sechat.Data.Models;

public record Message : BaseModel<long>
{
    public string Text { get; set; } = string.Empty;

    public long RoonId { get; set; }
    public Room? Room { get; set; }
}
