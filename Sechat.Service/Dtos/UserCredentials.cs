using FluentValidation;
using Sechat.Service.Configuration;

namespace Sechat.Service.Dtos;

public class UserCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UserCredentialsFormValidation : AbstractValidator<UserCredentials>
{
    public UserCredentialsFormValidation()
    {
        _ = RuleFor(x => x.Username).NotEmpty().MaximumLength(AppConstants.StringLength.UserNameMax);
        _ = RuleFor(x => x.Password).NotEmpty().MaximumLength(AppConstants.StringLength.PasswordMax);
    }
}

public class SignUpDetails
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string ReferralPass { get; set; }
}

public class SignUpDetailsValidation : AbstractValidator<SignUpDetails>
{
    public SignUpDetailsValidation()
    {
        _ = RuleFor(x => x.Username).NotEmpty().MaximumLength(AppConstants.StringLength.UserNameMax);
        _ = RuleFor(x => x.Password).NotEmpty().MaximumLength(AppConstants.StringLength.PasswordMax);
        _ = RuleFor(x => x.ReferralPass).NotEmpty().MinimumLength(AppConstants.StringLength.PasswordMin).MaximumLength(AppConstants.StringLength.PasswordMax);
    }
}
