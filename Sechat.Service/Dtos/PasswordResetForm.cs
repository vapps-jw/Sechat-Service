using FluentValidation;
using Sechat.Service.Configuration;

namespace Sechat.Service.Dtos;

public class PasswordResetForm
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class PasswordResetFormValidation : AbstractValidator<PasswordResetForm>
{
    public PasswordResetFormValidation()
    {
        _ = RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(AppConstants.StringLengths.PasswordMin).MaximumLength(AppConstants.StringLengths.PasswordMax);
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress();
        _ = RuleFor(x => x.Token).NotEmpty();
    }
}
