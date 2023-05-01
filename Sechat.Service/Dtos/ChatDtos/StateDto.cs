﻿using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

public class StateDto
{
    public List<RoomDto> Rooms { get; set; } = new();
    public List<UserContactDto> UserContacts { get; set; } = new();
}
