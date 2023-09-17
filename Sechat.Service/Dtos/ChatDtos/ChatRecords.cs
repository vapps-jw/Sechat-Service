using FluentValidation;
using Sechat.Service.Configuration;
using System;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.ChatDtos;

// Basic

public record ResourceId(long Id);
public record ResourceGuid(string Id);
public record StringMessage(string Message);
public record BooleanMessage(bool Message);
public record StringUserMessage(string UserName, string Message);
public record BoolUserMessage(string UserName, string Message);

// Direct Messages

public record ContactUpdateRequired(long ContactId);
public record DirectMessageId(long Id, long ContactId);
public record DirectMessagesViewed(long ContactId);
public record DirectMessageViewed(long ContactId, long MessageId);
public record IncomingDirectMessage(string Text, string Recipient);

public class IncomingDirectMessageValidation : AbstractValidator<IncomingDirectMessage>
{
    public IncomingDirectMessageValidation() => _ = RuleFor(x => x.Text).NotEmpty().MaximumLength(AppConstants.StringLength.Max);
}
// Rooms

public record MessageId(long Id, string RoomId);
public record RoomUserActionMessage(string RoomId, string UserName);
public record RoomMessageUserActionMessage(string RoomId, long MessageId, string UserName);
public record RoomIdsMessage(List<string> RoomIds);
public record IncomingMessage(string Text, string RoomId);

public class IncomingMessageValidation : AbstractValidator<IncomingMessage>
{
    public IncomingMessageValidation() => _ = RuleFor(x => x.Text).NotEmpty().MaximumLength(AppConstants.StringLength.Max);
}

public record CreateRoomMessage(string RoomName);
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