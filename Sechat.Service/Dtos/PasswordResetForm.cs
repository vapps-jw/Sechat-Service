using FluentValidation;

namespace Sechat.Service.Dtos;

public class PasswordResetForm
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class PasswordResetFormValidation : AbstractValidator<PasswordResetForm>
{
    public PasswordResetFormValidation()
    {
        _ = RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(30);
        _ = RuleFor(x => x.UserId).NotEmpty();
        _ = RuleFor(x => x.Token).NotEmpty();
    }
}
