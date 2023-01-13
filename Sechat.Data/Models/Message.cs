﻿namespace Sechat.Data.Models;

public record Message : BaseModel<long>
{
    public string IdSentBy { get; set; } = string.Empty;
    public string NameSentBy { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public long RoomId { get; set; }
    public Room Room { get; set; }
}
