using FluentValidation;
using Sechat.Service.Configuration;

namespace Sechat.Service.Dtos;

public class ReferralPass
{
    public string PassPhrase { get; set; } = string.Empty;
}

public class ReferallPassValidation : AbstractValidator<ReferralPass>
{
    public ReferallPassValidation() => _ = RuleFor(x => x.PassPhrase).NotEmpty().MinimumLength(AppConstants.StringLength.PasswordMin).MaximumLength(AppConstants.StringLength.PasswordMax);
}
