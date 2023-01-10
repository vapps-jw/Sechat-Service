using FluentValidation;

namespace Sechat.Service.Dtos;

public class PasswordForm
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public class PasswordFormValidation : AbstractValidator<PasswordForm>
{
    public PasswordFormValidation()
    {
        _ = RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(30);
        _ = RuleFor(x => x.OldPassword).NotEmpty().MinimumLength(8).MaximumLength(30);
    }
}
