using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

public class RoomDto
{
    public string Id { get; set; }
    public string CreatorName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public List<RoomMessageDto> Messages { get; set; } = new();
    public List<RoomMemberDto> Members { get; set; } = new();
}
