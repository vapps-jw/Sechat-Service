using System;

namespace Sechat.Service.Dtos.ChatDtos;

public class RoomMessageDto
{
    public string IdSentBy { get; set; } = string.Empty;
    public string NameSentBy { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    public string RoomId { get; set; } = string.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
