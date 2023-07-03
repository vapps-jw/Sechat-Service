using FluentValidation;
using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

// Basic

public record ResourceId(long Id);
public record ResourceGuid(string Id);

// Direct Messages

public record DirectMessageId(long Id, long ContactId);
public record DirectMessagesViewed(long ContactId);
public record DirectMessageViewed(long ContactId, long MessageId);
public record IncomingDirectMessage(string Text, string Recipient);

// Rooms

public record MessageId(long Id, string RoomId);
public record RoomUserActionMessage(string RoomId, string UserName);
public record RoomMessageUserActionMessage(string RoomId, long MessageId, string UserName);
public record StringMessage(string Message);
public record StringUserMessage(string UserName, string Message);
public record RoomIdsMessage(List<string> RoomIds);
public record IncomingMessage(string Text, string RoomId);

public class IncomingMessageValidation : AbstractValidator<IncomingMessage>
{
    public IncomingMessageValidation() => _ = RuleFor(x => x.Text).NotEmpty().MaximumLength(5000);
}

public record CreateRoomMessage(string RoomName, bool UserEncrypted);
public class CreateRoomMessageValidation : AbstractValidator<CreateRoomMessage>
{
    public CreateRoomMessageValidation() => _ = RuleFor(x => x.RoomName).NotEmpty().MaximumLength(20);
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