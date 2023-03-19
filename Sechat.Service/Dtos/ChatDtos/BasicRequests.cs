namespace Sechat.Service.Dtos.ChatDtos;

public record RoomMemberUpdateRequest(string UserName, string RoomId, long connectionId);
public record LeaveRoomRequest(string RoomId);

