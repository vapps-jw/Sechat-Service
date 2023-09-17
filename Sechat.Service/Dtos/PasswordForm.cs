﻿using FluentValidation;
using Sechat.Service.Configuration;

namespace Sechat.Service.Dtos;

public class PasswordForm
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}

public class PasswordFormValidation : AbstractValidator<PasswordForm>
{
    public PasswordFormValidation() => _ = RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(AppConstants.StringLength.PasswordMin).MaximumLength(AppConstants.StringLength.PasswordMax);
}
