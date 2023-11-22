using FluentValidation;
using Sechat.Service.Configuration;

namespace Sechat.Service.Dtos;

public class ChangePasswordForm
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public class PasswordFormValidation : AbstractValidator<ChangePasswordForm>
{
    public PasswordFormValidation() => _ = RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(AppConstants.StringLength.PasswordMin).MaximumLength(AppConstants.StringLength.PasswordMax);
}

