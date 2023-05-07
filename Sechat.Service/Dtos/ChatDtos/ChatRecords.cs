using FluentValidation;
using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

public record ResourceId(long Id);
public record ResourceGuid(string Id);
public record RoomUserActionMessage(string RoomId, string UserName);
public record RoomMessageUserActionMessage(string RoomId, long MessageId, string UserName);
public record StringMessage(string Message);
public record StringUserMessage(string UserName, string Message);

public record RoomIdsMessage(List<string> RoomIds);

public record IncomingMessage(string Text, string RoomId);
public class IncomingMessageValidation : AbstractValidator<IncomingMessage>
{
    public IncomingMessageValidation() => _ = RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
}

public record RoomNameMessage(string RoomName);
public class RoomNameMessageValidation : AbstractValidator<RoomNameMessage>
{
    public RoomNameMessageValidation() => _ = RuleFor(x => x.RoomName).NotEmpty().MaximumLength(25);
}

public record RoomMemberUpdateRequest(string UserName, string RoomId, long connectionId);
public record RoomRequest(string RoomId);

public record LastMessageInTheRoom(string RoomId, DateTime LastMessage);
public record GetNewMessagesRequest(List<LastMessageInTheRoom> LastMessageInTheRooms);

public record ConnectionRequestDto(string Username);
public class ConnectionRequestDtoValidation : AbstractValidator<ConnectionRequestDto>
{
    public ConnectionRequestDtoValidation() => _ = RuleFor(x => x.Username).NotEmpty();
}