using FluentValidation;
using System.Collections.Generic;

namespace Sechat.Service.Dtos.SignalRDtos;

public record RoomIdMessage(string RoomId);
public record RoomIdsMessage(List<string> RoomIds);

public record IncomingMessage(string Text, string RoomId);
public class IncomingMessageValidation : AbstractValidator<IncomingMessage>
{
    public IncomingMessageValidation() => _ = RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
}

public record RoomNameMessage(string RoomName);
public class RoomNameMessageValidation : AbstractValidator<RoomNameMessage>
{
    public RoomNameMessageValidation() => _ = RuleFor(x => x.RoomName).NotEmpty().MaximumLength(50);
}