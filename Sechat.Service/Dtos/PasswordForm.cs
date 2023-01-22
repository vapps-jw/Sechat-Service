using FluentValidation;
using Sechat.Service.Utilities;

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
        _ = RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(AppConstants.StringLengths.PasswordMin).MaximumLength(AppConstants.StringLengths.PasswordMax);
        _ = RuleFor(x => x.OldPassword).NotEmpty().MinimumLength(AppConstants.StringLengths.PasswordMin).MaximumLength(AppConstants.StringLengths.PasswordMax);
    }
}
