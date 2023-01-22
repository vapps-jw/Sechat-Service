using FluentValidation;

namespace Sechat.Service.Dtos.ChatDtos;

public class InvitationDto
{
    public string Username { get; set; }
}

public class InvitationDtoValidation : AbstractValidator<InvitationDto>
{
    public InvitationDtoValidation() => _ = RuleFor(x => x.Username).NotEmpty();
}
