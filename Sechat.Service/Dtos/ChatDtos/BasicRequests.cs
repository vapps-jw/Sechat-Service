using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

public record RoomMemberUpdateRequest(string UserName, string RoomId, long connectionId);
public record LeaveRoomRequest(string RoomId);

public record LastMessageInTheRoom(string RoomId, DateTime LastMessage);
public record GetNewMessagesRequest(List<LastMessageInTheRoom> LastMessageInTheRooms);

