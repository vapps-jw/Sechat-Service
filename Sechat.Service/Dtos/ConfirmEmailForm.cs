using FluentValidation;

namespace Sechat.Service.Dtos;

public class ConfirmEmailForm
{
    public string Email { get; set; }
    public string Token { get; set; }
}

public class ConfirmEmailFormValidation : AbstractValidator<ConfirmEmailForm>
{
    public ConfirmEmailFormValidation()
    {
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress();
        _ = RuleFor(x => x.Token).NotEmpty();
    }
}