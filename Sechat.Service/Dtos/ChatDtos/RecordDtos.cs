using FluentValidation;

namespace Sechat.Service.Dtos.ChatDtos;

public record ConnectionRequestDto(string Username);
public class ConnectionRequestDtoValidation : AbstractValidator<ConnectionRequestDto>
{
    public ConnectionRequestDtoValidation() => _ = RuleFor(x => x.Username).NotEmpty();
}
