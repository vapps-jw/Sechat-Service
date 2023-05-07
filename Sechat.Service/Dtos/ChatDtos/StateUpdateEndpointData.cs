using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

public class StateUpdateRequest
{
    public class RoomUpdateRequest
    {
        public DateTime LastMessageTimestamp { get; set; }
        public long RoomId { get; set; }
    }

    public List<RoomUpdateRequest> RoomUpdateReuqests { get; set; } = new();
    public List<int> CurrentContactRequests { get; set; } = new();
}

public class StateUpdateResponse
{
    public class RoomUpdateResponse
    {
        public long RoomId { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public List<RoomMessageDto> Messages { get; set; } = new();
        public List<RoomMemberDto> Members { get; set; } = new();
    }

    public List<RoomUpdateResponse> RoomUpdateResponses { get; set; } = new();
    public List<UserContactDto> NewContacts { get; set; } = new();
}

