using FluentValidation;

namespace Sechat.Service.Dtos;

public class ConfirmEmailForm
{
    public string UserName { get; set; }
    public string Token { get; set; }
    public string Email { get; set; }
}

public class ConfirmEmailFormValidation : AbstractValidator<ConfirmEmailForm>
{
    public ConfirmEmailFormValidation()
    {
        _ = RuleFor(x => x.UserName).NotEmpty();
        _ = RuleFor(x => x.Token).NotEmpty();
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}