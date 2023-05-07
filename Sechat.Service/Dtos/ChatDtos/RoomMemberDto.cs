using System;

namespace Sechat.Service.Dtos.ChatDtos;

public class RoomMemberDto
{
    public string UserName { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}
