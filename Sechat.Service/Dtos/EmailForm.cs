using FluentValidation;

namespace Sechat.Service.Dtos;

public class EmailForm
{
    public string Email { get; set; }
}

public class UserEmailFormValidation : AbstractValidator<EmailForm>
{
    public UserEmailFormValidation() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
}
