using System;

namespace Sechat.Service.Dtos.ChatDtos;

public class RoomMemberDto
{
    public string Id { get; set; }
    public string UserName { get; set; }

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;
}
