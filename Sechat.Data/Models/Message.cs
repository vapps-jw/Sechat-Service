using System.ComponentModel.DataAnnotations;

namespace Sechat.Data.Models;

public record Message : BaseModel<long>
{
    public string IdSentBy { get; set; } = string.Empty;
    public string NameSentBy { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    public string RoomId { get; set; }
    public Room Room { get; set; }
}
