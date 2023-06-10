using System;

namespace Sechat.Service.Dtos.ChatDtos;

public class RoomUpdateRequest
{
    public DateTime LastMessageTimestamp { get; set; }
    public string RoomId { get; set; }
}

