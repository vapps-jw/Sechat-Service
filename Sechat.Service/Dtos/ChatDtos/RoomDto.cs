﻿using System;
using System.Collections.Generic;
using Sechat.Service.Dtos.Messages;

namespace Sechat.Service.Dtos.ChatDtos;

public class RoomDto
{
    public string Id { get; set; }
    public string CreatorName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public List<MessageDto> Messages { get; set; } = new();
    public List<RoomMemberDto> Members { get; set; } = new();
}
