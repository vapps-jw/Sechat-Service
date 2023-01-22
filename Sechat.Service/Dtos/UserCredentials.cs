using FluentValidation;
using Sechat.Service.Utilities;

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
        _ = RuleFor(x => x.Username).NotEmpty().MinimumLength(AppConstants.StringLengths.UsernameMin).MaximumLength(AppConstants.StringLengths.UsernameMax);
        _ = RuleFor(x => x.Password).NotEmpty().MinimumLength(AppConstants.StringLengths.PasswordMin).MaximumLength(AppConstants.StringLengths.PasswordMax);
    }
}
