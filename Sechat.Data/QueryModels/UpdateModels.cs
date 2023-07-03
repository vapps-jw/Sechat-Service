namespace Sechat.Data.QueryModels;
public record GetRoomUpdate(string RoomId, long LastMessage);
public record GetContactUpdate(long ContactId, long LastMessage);