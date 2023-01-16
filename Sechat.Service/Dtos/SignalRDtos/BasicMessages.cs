using System.Collections.Generic;

namespace Sechat.Service.Dtos.SignalRDtos;

public record RoomIdMessage(string RoomId);
public record RoomNameMessage(string RoomName);
public record IncomingMessage(string Text, string RoomId);
public record RoomIdsMessage(List<string> RoomIds);