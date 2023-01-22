using FluentValidation;

namespace Sechat.Service.Dtos.ChatDtos;

public record UserConnectionDto(string Inviter, string Invited, bool Approved);

public record ConnectionRequestDto(string Username);
public class ConnectionRequestDtoValidation : AbstractValidator<ConnectionRequestDto>
{
    public ConnectionRequestDtoValidation() => _ = RuleFor(x => x.Username).NotEmpty();
}
